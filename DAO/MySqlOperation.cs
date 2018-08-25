using DataManagement.Standard.Enums;
using DataManagement.Standard.Exceptions;
using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using DataManagement.Standard.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Standard.DAO
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

        public Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

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
                    dataTable = new DataTable();
                    dataTable.Load(Command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
                Logger.Info(string.Format("Execution of stored procedure {0} using connection {1} has finished successfully.", storedProcedure, connectionToUse));
            }
            catch (MySqlException mySqlException)
            {
                Logger.Error(mySqlException);
                throw mySqlException;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return new Result(dataTable, false, true);
        }

        public Result ExecuteProcedure<T, TKey>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable<TKey>, new() where TKey : struct
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
                    PerformTableConsolidation<T, TKey>(connectionToUse, false);
                }
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.Insert)
                    {
                        SetParameters<T, TKey>(obj, transactionType, false);
                        Command.ExecuteNonQuery();
                    }
                    else if (transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                    {
                        SetParameters<T, TKey>(obj, transactionType, true);
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        if (transactionType == TransactionTypes.Select)
                        {
                            SetParameters<T, TKey>(obj, transactionType, true);
                        }
                        dataTable = new DataTable();
                        dataTable.Load(Command.ExecuteReader());
                        dataTable.TableName = tableName;
                    }
                }
                Logger.Info(string.Format("Execution {0} for object {1} using connection {2} has finished successfully.", transactionType.ToString(), typeof(T), connectionToUse));
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (Manager.AutoCreateStoredProcedures)
                {
                    Logger.Warn(string.Format("Stored Procedure for {0} not found. Creating...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T, TKey>(transactionType, false), connectionToUse, false);
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
                    Logger.Warn(string.Format("Table {0} not found. Creating...", obj.DataBaseTableName));
                    ProcessTable<T, TKey>(connectionToUse, false);
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
                    ExecuteScalar(GetTransactionTextForProcedure<T, TKey>(transactionType, true), connectionToUse, false);
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
                    ProcessTable<T, TKey>(connectionToUse, true);
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

            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable, false, true);
        }

        public Result ExecuteProcedure<T, TKey>(List<T> list, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable<TKey>, new() where TKey : struct 
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
                    PerformTableConsolidation<T, TKey>(connectionToUse, false);
                }
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.InsertList)
                    {
                        SetParameters<T, TKey>(list, transactionType);
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
                    ExecuteScalar(GetTransactionTextForProcedure<T, TKey>(transactionType, false), connectionToUse, false);
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
                    Logger.Warn(string.Format("Table {0} not found. Creating...", obj.DataBaseTableName));
                    ProcessTable<T, TKey>(connectionToUse, false);
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
                    ExecuteScalar(GetTransactionTextForProcedure<T, TKey>(transactionType, true), connectionToUse, false);
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
                    ProcessTable<T, TKey>(connectionToUse, true);
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

            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable, false, true);
        }

        public void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse)
        {
            if (!Manager.EnableLogInDatabase)
            {
                return;
            }

            Logger.Info(string.Format("Saving log information into the database."));
            Log newLog = NewLog(dataBaseTableName, transactionType);
            ExecuteProcedure<Log, Guid>(newLog, newLog.DataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}

