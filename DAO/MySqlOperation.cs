using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

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

        internal override Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
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

        internal override Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) 
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

        private void ProcessTableCreation<T>(string connectionToUse) where T : IManageable, new()
        {
            ExecuteNonQuery(Creation.GetCreateTableQuery<T>(ConnectionTypes.MySQL), connectionToUse);
            VerifyForeignTables(typeof(T), connectionToUse);
            string foreignKeyQuery = Creation.GetCreateForeignKeysQuery(typeof(T), ConnectionTypes.MySQL);

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteNonQuery(Creation.GetCreateForeignKeysQuery(typeof(T), ConnectionTypes.MySQL), connectionToUse);
            }
        }

        private void VerifyForeignTables(Type type, string connectionToUse)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                if (!CheckIfTableExists(foreignModel.DataBaseTableName, connectionToUse))
                {
                    ExecuteNonQuery(Creation.GetCreateTableQuery(foreignModel.GetType(), ConnectionTypes.MySQL), connectionToUse);
                    VerifyForeignTables(foreignModel.GetType(), connectionToUse);
                    string foreignKeyQuery = Creation.GetCreateForeignKeysQuery(foreignModel.GetType(), ConnectionTypes.MySQL);

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

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType)
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
                Parametros = GetStringParameters(command)
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }

        private string GetStringParameters(MySqlCommand mySqlCommand)
        {
            StringBuilder builder = new StringBuilder();

            foreach (MySqlParameter parametro in mySqlCommand.Parameters)
            {
                if (parametro.Value != null)
                {
                    builder.AppendFormat("{0}: {1}|", parametro.ParameterName, parametro.Value);
                }
            }

            return builder.ToString();
        }

        private void SetParameters<T>(T obj, TransactionTypes transactionType, MySqlCommand mySqlCommand)
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo entonces se brinca la propiedad.
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(UnlinkedProperty)) != null) continue;

                if (transactionType == TransactionTypes.Delete)
                {
                    if (propertyInfo.Name == "Id")
                    {
                        mySqlCommand.Parameters.AddWithValue("_id", propertyInfo.GetValue(obj));
                        break;
                    }
                }
                else
                {
                    mySqlCommand.Parameters.AddWithValue("_" + propertyInfo.Name, propertyInfo.GetValue(obj));

                }
            }
        }
    }
}
