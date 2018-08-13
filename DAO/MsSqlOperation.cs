using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DataManagement.DAO
{
    internal class MsSqlOperation : Operation
    {
        private SqlCommand command;

        internal override int ExecuteNonQuery(string transaction, string connectionToUse)
        {
            try
            {
                using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    command = new SqlCommand(transaction, connection);
                    command.CommandType = CommandType.Text;
                    return command.ExecuteNonQuery();
                }
            }
            catch (SqlException mssqle)
            {
                throw mssqle;
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
                using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    command = new SqlCommand(storedProcedure, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null) SetParameters(parameters, msSqlCommand: command);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (SqlException mssqle)
            {
                throw mssqle;
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
            catch (SqlException mssqle) when (mssqle.Number == 2812)
            {
                if (AutoCreateStoredProcedures)
                {
                    ExecuteNonQuery(GetTransactionTextForStores<T>(transactionType, ConnectionTypes.MSSQL), connectionToUse);
                    goto Start;
                }
                else
                {
                    throw mssqle;
                }
            }
            catch (SqlException mssqle) when (mssqle.Number == 208)
            {
                if (AutoCreateTables)
                {
                    ExecuteNonQuery(Creation.GetCreateTableQuery<T>(ConnectionTypes.MSSQL), connectionToUse);
                    VerifyForeignTables(typeof(T), connectionToUse);
                    string foreignKeyQuery = Creation.GetCreateForeignKeysQuery(typeof(T), ConnectionTypes.MSSQL);

                    if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                    {
                        ExecuteNonQuery(Creation.GetCreateForeignKeysQuery(typeof(T), ConnectionTypes.MSSQL), connectionToUse);
                    }

                    goto Start;
                }
                else
                {
                    throw mssqle;
                }
            }

            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable);
        }

        private void VerifyForeignTables(Type type, string connectionToUse)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                if (!CheckIfTableExists(foreignModel.DataBaseTableName, connectionToUse))
                {
                    ExecuteNonQuery(Creation.GetCreateTableQuery(foreignModel.GetType(), ConnectionTypes.MSSQL), connectionToUse);
                    VerifyForeignTables(foreignModel.GetType(), connectionToUse);
                    string foreignKeyQuery = Creation.GetCreateForeignKeysQuery(foreignModel.GetType(), ConnectionTypes.MSSQL);

                    if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                    {
                        ExecuteNonQuery(foreignKeyQuery, connectionToUse);
                    }
                }
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            string query = string.Format("SELECT name FROM sysobjects WHERE name='{0}' AND xtype='U'", tableName);

            if (ExecuteNonQuery(query, connectionToUse) <= 0)
            {
                return false;
            }
            return true;
        }

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType) where T : IManageable
        {
            DataTable dataTable = null;

            using (SqlConnection connection = Connection.OpenMsSqlConnection(connectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                command = new SqlCommand(string.Format("{0}.{1}{2}{3}", obj.Schema, StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
                command.CommandType = CommandType.StoredProcedure;

                if (transactionType == TransactionTypes.Insert || transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                {
                    SetParameters(obj, transactionType, msSqlCommand: command);
                    command.ExecuteNonQuery();
                }
                else
                {
                    if (transactionType == TransactionTypes.Select)
                    {
                        SetParameters(obj, transactionType, msSqlCommand: command);
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
                Parametros = GetStringParameters(null, command)
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }
    }
}
