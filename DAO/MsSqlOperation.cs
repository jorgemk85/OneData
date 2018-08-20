using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DataManagement.DAO
{
    internal class MsSqlOperation : Operation, IOperable
    {
        const int ERR_STORED_PROCEDURE_NOT_FOUND = 2812;
        const int ERR_OBJECT_NOT_FOUND = 208;
        const int ERR_INCORRECT_NUMBER_OF_ARGUMENTS = 8144;

        public MsSqlOperation() : base()
        {
            ConnectionType = ConnectionTypes.MSSQL;
            Creator = new MsSqlCreation();
            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = '{0}{1}'";
            QueryForColumnDefinition = string.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", string.Format("{0}{1}", TablePrefix, "{0}"));
            QueryForKeyDefinition = string.Format("SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{0}' AND COLUMN_NAME != 'Id'", string.Format("{0}{1}", TablePrefix, "{0}"));
        }

        public Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

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
                    dataTable = new DataTable();
                    dataTable.Load(Command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (SqlException mySqlException)
            {
                Logger.Error(mySqlException);
                throw mySqlException;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return new Result(dataTable);
        }

        public Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new()
        {
            DataTable dataTable = null;
            bool overrideConsolidation = false;

        Start:
            try
            {
                Logger.Info(string.Format("Starting {0} execution for object {1} using connection {2}", transactionType.ToString(), nameof(obj), connectionToUse));
                if (ConstantTableConsolidation && (Manager.IsDebug || OverrideOnlyInDebug) && !overrideConsolidation)
                {
                    PerformTableConsolidation<T>(connectionToUse, false);
                }
                using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = string.Format("{0}.{1}{2}{3}", obj.Schema, StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType));

                    if (transactionType == TransactionTypes.Insert || transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                    {
                        SetParameters(obj, transactionType);
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        if (transactionType == TransactionTypes.Select)
                        {
                            SetParameters(obj, transactionType);
                        }
                        dataTable = new DataTable();
                        dataTable.Load(Command.ExecuteReader());
                        dataTable.TableName = tableName;
                    }
                }
            }
            catch (SqlException sqlException) when (sqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (AutoCreateStoredProcedures)
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
                if (AutoCreateTables)
                {
                    Logger.Warn(string.Format("Table {0} not found. Creating...", obj.DataBaseTableName));
                    ProcessTable<T>(connectionToUse, false);
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

            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable);
        }

        public void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse)
        {
            if (!EnableLog)
            {
                return;
            }

            Logger.Info(string.Format("Saving log information into the database."));
            Log newLog = NewLog(dataBaseTableName, transactionType);
            ExecuteProcedure(newLog, newLog.DataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}
