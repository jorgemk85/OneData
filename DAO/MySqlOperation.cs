using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.DAO
{
    internal class MySqlOperation : Operation, IOperable
    {
        const int ERR_TABLE_NOT_FOUND = 1146;
        const int ERR_STORED_PROCEDURE_NOT_FOUND = 1305;
        const int ERR_INCORRECT_NUMBER_OF_ARGUMENTS = 1318;
        const int ERR_UNKOWN_COLUMN = 1054;
        const int ERR_NO_DEFAULT_VALUE_IN_FIELD = 1364;

        public MySqlOperation() : base()
        {
            ConnectionType = ConnectionTypes.MySQL;
            Creator = new MySqlCreation();
            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}{1}'";
            QueryForColumnDefinition = string.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", string.Format("{0}{1}", Manager.TablePrefix, "{0}"));
            QueryForKeyDefinition = string.Format("SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{0}' AND COLUMN_NAME != 'Id'", string.Format("{0}{1}", Manager.TablePrefix, "{0}"));
        }

        public DataSet ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataSet dataSet = new DataSet();

            try
            {
                Logger.Info(string.Format("Starting execution of stored procedure {0} using connection {1}.", storedProcedure, connectionToUse));
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = storedProcedure;

                    if (parameters != null) SetParameters(parameters);
                    var adapter = new MySqlDataAdapter((MySqlCommand)Command);
                    adapter.Fill(dataSet);
                }
                Logger.Info(string.Format("Execution of stored procedure {0} using connection {1} has finished successfully.", storedProcedure, connectionToUse));
            }
            catch (MySqlException mySqlException)
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
            bool nextTryRepairStoredProcedure = false;

            Start:
            try
            {
                Logger.Info(string.Format("Starting {0} execution for object {1} using connection {2}", transactionType.ToString(), typeof(T), connectionToUse));
                if (Manager.ConstantTableConsolidation && (Manager.IsDebug || Manager.OverrideOnlyInDebug) && !overrideConsolidation && !obj.GetType().Equals(typeof(Log)))
                {
                    PerformTableConsolidation<T>(connectionToUse, false);
                }
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.Insert)
                    {
                        SetParameters<T>(obj, transactionType, false);
                        Command.ExecuteNonQuery();
                    }
                    else if (transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                    {
                        SetParameters<T>(obj, transactionType, true);
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
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (Manager.AutoCreateStoredProcedures)
                {
                    Logger.Warn(string.Format("Stored Procedure for {0} not found. Creating...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_TABLE_NOT_FOUND)
            {
                if (Manager.AutoCreateTables)
                {
                    Logger.Warn(string.Format("Table {0} not found. Creating...", Cope<T>.ModelComposition.TableName));
                    ProcessTable<T>(connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS || (mySqlException.Number == ERR_UNKOWN_COLUMN && nextTryRepairStoredProcedure))
            {
                nextTryRepairStoredProcedure = false;
                if (Manager.AutoAlterStoredProcedures)
                {
                    Logger.Warn(string.Format("Incorrect number of arguments or unkown column related to the {0} stored procedure. Modifying...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, true), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_UNKOWN_COLUMN || mySqlException.Number == ERR_NO_DEFAULT_VALUE_IN_FIELD)
            {
                nextTryRepairStoredProcedure = true;
                if (Manager.AutoAlterTables)
                {
                    ProcessTable<T>(connectionToUse, true);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
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
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.InsertMassive)
                    {
                        SetParameters<T>(list, transactionType);
                        Command.ExecuteNonQuery();
                    }
                }
                Logger.Info(string.Format("Execution {0} for list {1} using connection {2} has finished successfully.", transactionType.ToString(), typeof(T), connectionToUse));
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (Manager.AutoCreateStoredProcedures)
                {
                    Logger.Warn(string.Format("Stored Procedure for {0} not found. Creating...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_TABLE_NOT_FOUND)
            {
                if (Manager.AutoCreateTables)
                {
                    Logger.Warn(string.Format("Table {0} not found. Creating...", Cope<T>.ModelComposition.TableName));
                    ProcessTable<T>(connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS)
            {
                if (Manager.AutoAlterStoredProcedures)
                {
                    Logger.Warn(string.Format("Incorrect number of arguments related to the {0} stored procedure. Modifying...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, true), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_UNKOWN_COLUMN || mySqlException.Number == ERR_NO_DEFAULT_VALUE_IN_FIELD)
            {
                if (Manager.AutoAlterTables)
                {
                    ProcessTable<T>(connectionToUse, true);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
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

