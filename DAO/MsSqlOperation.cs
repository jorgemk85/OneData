using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace DataManagement.DAO
{
    internal class MsSqlOperation : Operation, IOperable
    {
        const int ERR_STORED_PROCEDURE_NOT_FOUND = 2812;
        const int ERR_OBJECT_NOT_FOUND = 208;
        const int ERR_INCORRECT_NUMBER_OF_ARGUMENTS = 8144;
        const int ERR_EXPECTED_PARAMETER_NOT_SUPPLIED = 201;
        const int ERR_CANNOT_INSERT_EXPLICIT_VALUE_FOR_IDENTITY = 544;
        const int ERR_CANNOT_UPDATE_IDENTITY_VALUE = 8102;

        public MsSqlOperation() : base()
        {
            _connectionType = ConnectionTypes.MSSQL;
            _creator = new MsSqlCreation();
            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = '{0}{1}'";
            QueryForColumnDefinition = string.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", string.Format("{0}{1}", Manager.TablePrefix, "{0}"));
            QueryForKeyDefinition = string.Format("SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{0}' AND COLUMN_NAME != 'Id'", string.Format("{0}{1}", Manager.TablePrefix, "{0}"));
        }

        public DataSet ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataSet dataSet = new DataSet();

            try
            {
                Logger.Info(string.Format("Starting execution of stored procedure {0} using connection {1}", storedProcedure, connectionToUse));
                using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    _command = connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = storedProcedure;
                    if (parameters != null) SetParameters(parameters);
                    var adapter = new SqlDataAdapter((SqlCommand)_command);
                    adapter.Fill(dataSet);
                }
                Logger.Info(string.Format("Execution of stored procedure {0} using connection {1} has finished successfully.", storedProcedure, connectionToUse));
            }
            catch (SqlException mySqlException)
            {
                Logger.Error(mySqlException);
                throw mySqlException;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return dataSet;
        }

        public Result<T> ExecuteProcedure<T>(object obj, string connectionToUse, TransactionTypes transactionType, bool logTransaction) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = null;
            bool overrideConsolidation = false;

            Start:
            try
            {
                Logger.Info(string.Format("Starting {0} execution for object {1} using connection {2}", transactionType.ToString(), typeof(T), connectionToUse));
                if (Manager.ConstantTableConsolidation && (Manager.IsDebug || Manager.OverrideOnlyInDebug) && !overrideConsolidation && !obj.GetType().Equals(typeof(Log)))
                {
                    PerformTableConsolidation<T>(connectionToUse, false);
                }

                switch (transactionType)
                {
                    case TransactionTypes.Select:
                        result = ExecuteProcedure((T)obj, connectionToUse, transactionType);
                        break;
                    case TransactionTypes.SelectQuery:
                        throw new NotImplementedException();
                    case TransactionTypes.SelectAll:
                        result = ExecuteProcedure((T)obj, connectionToUse, transactionType);
                        break;
                    case TransactionTypes.Delete:
                        result = ExecuteProcedure((T)obj, connectionToUse, transactionType);
                        break;
                    case TransactionTypes.Insert:
                        result = ExecuteProcedure((T)obj, connectionToUse, transactionType);
                        break;
                    case TransactionTypes.InsertMassive:
                        result = ExecuteProcedure((IEnumerable<T>)obj, connectionToUse, transactionType);
                        break;
                    case TransactionTypes.Update:
                        result = ExecuteProcedure((T)obj, connectionToUse, transactionType);
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }

                Logger.Info(string.Format("Execution {0} for object {1} using connection {2} has finished successfully.", transactionType.ToString(), typeof(T), connectionToUse));
            }
            catch (SqlException sqlException) when (sqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (Manager.AutoCreateStoredProcedures)
                {
                    Logger.Warn(string.Format("Stored Procedure for {0} not found. Creating...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(sqlException);
                throw;
            }
            catch (SqlException sqlException) when (sqlException.Number == ERR_OBJECT_NOT_FOUND)
            {
                if (Manager.AutoCreateTables)
                {
                    Logger.Warn(string.Format("Table {0} not found. Creating...", Cope<T>.ModelComposition.TableName));
                    ProcessTable<T>(connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(sqlException);
                throw;
            }
            catch (SqlException sqlException) when (sqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS ||
                                                    sqlException.Number == ERR_CANNOT_INSERT_EXPLICIT_VALUE_FOR_IDENTITY ||
                                                    sqlException.Number == ERR_EXPECTED_PARAMETER_NOT_SUPPLIED ||
                                                    sqlException.Number == ERR_CANNOT_UPDATE_IDENTITY_VALUE)
            {
                if (Manager.AutoAlterStoredProcedures)
                {
                    Logger.Warn(string.Format("Incorrect number of arguments or is identity explicit value related to the {0} stored procedure. Modifying...", transactionType.ToString()));
                    PerformTableConsolidation<T>(connectionToUse, true);
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, true), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(sqlException);
                throw;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }

            if (logTransaction) LogTransaction(Cope<T>.ModelComposition.TableName, transactionType, connectionToUse);

            return result;
        }

        private Result<T> ExecuteProcedure<T>(T obj, string connectionToUse, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = string.Format("{0}.{1}{2}{3}", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                switch (transactionType)
                {
                    case TransactionTypes.Select:
                        SetParameters(obj, transactionType, true);
                        FillDictionaryWithReader(_command.ExecuteReader(), ref result);
                        break;
                    case TransactionTypes.SelectAll:
                        FillDictionaryWithReader(_command.ExecuteReader(), ref result);
                        break;
                    case TransactionTypes.Delete:
                        SetParameters(obj, transactionType, true);
                        _command.ExecuteNonQuery();
                        break;
                    case TransactionTypes.Insert:
                        SetParameters(obj, transactionType, false);
                        _command.ExecuteNonQuery();
                        break;
                    case TransactionTypes.Update:
                        SetParameters(obj, transactionType, true);
                        _command.ExecuteNonQuery();
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }
            }

            return result;
        }

        private Result<T> ExecuteProcedure<T>(IEnumerable<T> list, string connectionToUse, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = string.Format("{0}.{1}{2}{3}", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                switch (transactionType)
                {
                    case TransactionTypes.InsertMassive:
                        SetParameters(list, transactionType);
                        _command.ExecuteNonQuery();
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }
            }
            return new Result<T>(new Dictionary<dynamic, T>(), false, true);
        }

        public void LogTransaction(string tableName, TransactionTypes transactionType, string connectionToUse)
        {
            if (!Manager.EnableLogInDatabase)
            {
                return;
            }

            Logger.Info(string.Format("Saving log information into the database."));
            Log newLog = NewLog(tableName, transactionType);
            ExecuteProcedure<Log>(newLog, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}
