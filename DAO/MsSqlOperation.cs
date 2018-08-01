using DataManagement.BO;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DataManagement.DAO
{
    internal class MsSqlOperation : DbOperation
    {
        private SqlCommand command;

        public override Result EjecutarProcedimiento(string tableName, string storedProcedure, Parameter[] parameters, bool useAppConfig, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                using (SqlConnection connection = Connection.OpenMSSQLConnection(useAppConfig))
                {
                    if (connection.State != ConnectionState.Open) return new Result(exito: false, mensaje: "No se puede abrir la conexion con la base de datos.", titulo: "Error al intentar conectar.");
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
                Debug.WriteLine(mssqle.Message);
                return new Result(mssqle: mssqle);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                return new Result(ae: ae);
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.ExecuteStoredProcedure, useAppConfig);

            return new Result(true, dataTable);
        }

        public override Result ExecuteProcedure<T>(T obj, string tableName, TransactionTypes transactionType, bool useAppConfig, ConnectionTypes connectionType, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                dataTable = ConfigureConnectionAndExecuteCommand(obj, tableName, transactionType, useAppConfig);
            }
            catch (SqlException mse)
            {
                Debug.WriteLine(mse.Message);
                return new Result(mssqle: mse);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                return new Result(ae: ae);
            }

            if (logTransaction) LogTransaction(tableName, transactionType, useAppConfig);


            return new Result(true, dataTable, Tools.MsSqlParameterCollectionToList(command.Parameters));
        }

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, TransactionTypes transactionType, bool useAppConfig)
        {
            DataTable dataTable = null;

            using (SqlConnection connection = Connection.OpenMSSQLConnection(useAppConfig))
            {
                if (connection.State != ConnectionState.Open) throw new Exception("No se puede abrir la conexion con la base de datos.");
                command = new SqlCommand(string.Format("{0}{1}{2}{3}", (obj as Main).Schema + ".", StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
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

        private void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, bool useAppConfig)
        {
            Log newLog = new Log
            {
                IdentificadorId = IdentificadorId,
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters(null, command)
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, TransactionTypes.Insert, useAppConfig, ConnectionTypes.MSSQL, false);
        }
    }
}
