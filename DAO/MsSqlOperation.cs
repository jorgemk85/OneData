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
            ConnectionType = ConnectionTypes.MSSQL;
            Creator = new MsSqlCreation();
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
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = storedProcedure;
                    if (parameters != null) SetParameters(parameters);
                    var adapter = new SqlDataAdapter((SqlCommand)Command);
                    adapter.Fill(dataSet);
                }
                Logger.Info(string.Format("Execution of stored procedure {0} using connection {1} has finished successfully.", storedProcedure, connectionToUse));
            }
            catch (SqlException mySqlException)
            {
                Logger.Error(mySqlException);
                throw mySqlException;
            }

            if (logTransaction) LogTransaction<Log>(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return dataSet;
        }

        public Result<T> ExecuteProcedure<T>(T obj, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : Cope<T>, IManageable, new()
        {
            DataTable dataTable = null;
            bool overrideConsolidation = false;

            Start:
            try
            {
                Logger.Info(string.Format("Starting {0} execution for object {1} using connection {2}", transactionType.ToString(), typeof(T), connectionToUse));
                if (Manager.ConstantTableConsolidation && (Manager.IsDebug || Manager.OverrideOnlyInDebug) && !overrideConsolidation && !obj.GetType().Equals(typeof(Log)))
                {
                    PerformTableConsolidation<T>(connectionToUse, false);
                }
                using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}.{1}{2}{3}", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.Insert)
                    {
                        SetParameters(obj, transactionType, false);
                        Command.ExecuteNonQuery();
                    }
                    else if (transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                    {
                        SetParameters(obj, transactionType, true);
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        if (transactionType == TransactionTypes.Select)
                        {
                            SetParameters<T>(obj, transactionType, true);
                        }
                        dataTable = new DataTable();
                        dataTable.Load(Command.ExecuteReader());
                        dataTable.TableName = Cope<T>.ModelComposition.TableName;
                    }
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

            if (logTransaction) LogTransaction<T>(Cope<T>.ModelComposition.TableName, transactionType, connectionToUse);

            return new Result<T>(dataTable.ToDictionary<T>(Cope<T>.ModelComposition.PrimaryKeyProperty.Name, Cope<T>.ModelComposition.PrimaryKeyProperty.PropertyType), false, true);
        }

        public Result<T> ExecuteProcedure<T>(IEnumerable<T> list, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : Cope<T>, IManageable, new()
        {
            DataTable dataTable = null;
            bool overrideConsolidation = false;
            T obj = new T();

            Start:
            try
            {
                Logger.Info(string.Format("Starting {0} execution for list {1} using connection {2}", transactionType.ToString(), typeof(T), connectionToUse));
                if (Manager.ConstantTableConsolidation && (Manager.IsDebug || Manager.OverrideOnlyInDebug) && !overrideConsolidation && !obj.GetType().Equals(typeof(Log)))
                {
                    PerformTableConsolidation<T>(connectionToUse, false);
                }
                using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}.{1}{2}{3}", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.InsertMassive)
                    {
                        SetParameters(list, transactionType);
                        Command.ExecuteNonQuery();
                    }
                }
                Logger.Info(string.Format("Execution {0} for list {1} using connection {2} has finished successfully.", transactionType.ToString(), typeof(T), connectionToUse));
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
            catch (SqlException sqlException) when (sqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS)
            {
                if (Manager.AutoAlterStoredProcedures)
                {
                    Logger.Warn(string.Format("Incorrect number of arguments related to the {0} stored procedure. Modifying...", transactionType.ToString()));
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

            if (logTransaction) LogTransaction<T>(Cope<T>.ModelComposition.TableName, transactionType, connectionToUse);

            return new Result<T>(dataTable.ToDictionary<T>(Cope<T>.ModelComposition.PrimaryKeyProperty.Name, Cope<T>.ModelComposition.PrimaryKeyProperty.PropertyType), false, true);
        }

        public void LogTransaction<T>(string tableName, TransactionTypes transactionType, string connectionToUse)
        {
            if (!Manager.EnableLogInDatabase)
            {
                return;
            }

            Logger.Info(string.Format("Saving log information into the database."));
            Log newLog = NewLog<T>(tableName, transactionType);
            ExecuteProcedure(newLog, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}
