using DataAccess.BO;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;

namespace DataAccess.DAO
{
    internal class MySqlOperation : DbOperation
    {
        private MySqlCommand command;

        public override Result EjecutarProcedimiento(string tableName, string storedProcedure, Parameter[] parameters, bool useAppConfig, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                using (MySqlConnection connection = Connection.OpenConnection(useAppConfig))
                {
                    if (connection.State != ConnectionState.Open) return new Result(exito: false, mensaje: "No se puede abrir la conexion con la base de datos.", titulo: "Error al intentar conectar.");
                    command = new MySqlCommand(storedProcedure, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null) SetParameters(parameters, mySqlCommand: command);
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (MySqlException mse)
            {
                Debug.WriteLine(mse.Message);
                return new Result(mysqle: mse);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                return new Result(ae: ae);
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.SelectOther, useAppConfig);

            return new Result(true, dataTable);
        }

        public override Result ExecuteProcedure<T>(T obj, string tableName, TransactionTypes transactionType, bool useAppConfig, ConnectionTypes connectionType, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                dataTable = ConfigureConnectionAndExecuteCommand(obj, tableName, transactionType, useAppConfig);
            }
            catch (MySqlException mse)
            {
                Debug.WriteLine(mse.Message);
                return new Result(mysqle: mse);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                return new Result(ae: ae);
            }

            if (logTransaction) LogTransaction(tableName, transactionType, useAppConfig);

            return new Result(true, dataTable, Tools.MySqlParameterCollectionToList(command.Parameters));
        }

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, TransactionTypes transactionType, bool useAppConfig)
        {
            DataTable dataTable = null;

            using (MySqlConnection connection = Connection.OpenConnection(useAppConfig))
            {
                if (connection.State != ConnectionState.Open) throw new Exception("No se puede abrir la conexion con la base de datos.");
                command = new MySqlCommand(string.Format("{0}{1}{2}{3}", (obj as Main).Schema + ".", StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
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

        private void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, bool useAppConfig)
        {
            Log newLog = new Log
            {
                IdentificadorId = IdentificadorId,
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters(command, null)
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, TransactionTypes.Insert, useAppConfig, ConnectionTypes.MySQL, false);
        }
    }
}
