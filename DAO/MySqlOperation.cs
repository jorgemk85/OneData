using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace DataManagement.DAO
{
    internal class MySqlOperation : Operation
    {
        private MySqlCommand command;

        internal override int ExecuteNonQuery(string transaction, string connectionToUse)
        {
            try
            {
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    command = new MySqlCommand(transaction, connection);
                    command.CommandType = CommandType.Text;
                    return command.ExecuteNonQuery();
                }
            }
            catch (MySqlException mysqle)
            {
                throw mysqle;
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }
        }

        public override Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    command = new MySqlCommand(storedProcedure, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null) SetParameters(parameters, mySqlCommand: command);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (MySqlException mysqle)
            {
                throw mysqle;
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return new Result(dataTable);
        }

        public override Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true)
        {
            DataTable dataTable = null;

            Start:
            try
            {
                dataTable = ConfigureConnectionAndExecuteCommand(obj, tableName, connectionToUse, transactionType);
            }
            catch (MySqlException mysqle) when (mysqle.Number == 2812)
            {
                if (AutoCreateStoredProcedures)
                {
                    ExecuteNonQuery(GetTransactionTextForStores<T>(transactionType, ConnectionTypes.MySQL), connectionToUse);
                    goto Start;
                }
                else
                {
                    throw mysqle;
                }
            }
            catch (MySqlException mysqle) when (mysqle.Number == 208)
            {
                if (AutoCreateTables)
                {
                    ExecuteNonQuery(Creation.GetCreateTableQuery<T>(ConnectionTypes.MySQL), connectionToUse);
                    goto Start;
                }
                else
                {
                    throw mysqle;
                }
            }

            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable);
        }

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType) where T : IManageable
        {
            DataTable dataTable = null;

            using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new Exception("No se puede abrir la conexion con la base de datos.");
                command = new MySqlCommand(string.Format("{0}{1}{2}", StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
                command.CommandType = CommandType.StoredProcedure;

                if (transactionType == TransactionTypes.Insert || transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                {
                    SetParameters(obj, transactionType, mySqlCommand: command);
                    command.ExecuteNonQuery();
                }
                else
                {
                    if (transactionType == TransactionTypes.Select)
                    {
                        SetParameters(obj, transactionType, mySqlCommand: command);
                    }
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            return dataTable;
        }

        private void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse)
        {
            Log newLog = new Log
            {
                Ip = string.Empty,
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters(command, null)
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}
