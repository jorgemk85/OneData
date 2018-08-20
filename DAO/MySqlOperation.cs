using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using MySql.Data.MySqlClient;
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
        }

        public Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
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
            }
            catch (MySqlException mySqlException)
            {
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
                if (ConstantTableConsolidation && !overrideConsolidation)
                {
                    PerformTableConsolidation<T>(connectionToUse, false);
                }
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
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
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (AutoCreateStoredProcedures)
                {
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_TABLE_NOT_FOUND)
            {
                if (AutoCreateTables)
                {
                    ProcessTable<T>(connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS)
            {
                if (AutoAlterStoredProcedures)
                {
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, true), connectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_UNKOWN_COLUMN || mySqlException.Number == ERR_NO_DEFAULT_VALUE_IN_FIELD)
            {
                if (AutoAlterTables)
                {
                    ProcessTable<T>(connectionToUse, true);
                    overrideConsolidation = true;
                    goto Start;
                }
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

            ExecuteProcedure(NewLog(dataBaseTableName, transactionType), dataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}

