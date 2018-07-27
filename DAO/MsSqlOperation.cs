using DataAccess.BO;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DataAccess.DAO
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

            if (logTransaction) LogTransaction(tableName, QueryEvaluation.TransactionTypes.SelectOther, useAppConfig);

            return new Result(true, dataTable);
        }

        public override Result ExecuteProcedure<T>(T obj, string tableName, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig, QueryEvaluation.ConnectionTypes connectionType, bool logTransaction = true)
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

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig)
        {
            DataTable dataTable = null;

            using (SqlConnection connection = Connection.OpenMSSQLConnection(useAppConfig))
            {
                if (connection.State != ConnectionState.Open) throw new Exception("No se puede abrir la conexion con la base de datos.");
                command = new SqlCommand(string.Format("{0}{1}{2}{3}", (obj as Main).Schema + ".", StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
                command.CommandType = CommandType.StoredProcedure;

                if (transactionType == QueryEvaluation.TransactionTypes.Insert || transactionType == QueryEvaluation.TransactionTypes.Update || transactionType == QueryEvaluation.TransactionTypes.Delete)
                {
                    SetParameters(obj, transactionType, msSqlCommand: command);
                    command.ExecuteNonQuery();
                }
                else
                {
                    if (transactionType == QueryEvaluation.TransactionTypes.Select)
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

        private void LogTransaction(string dataBaseTableName, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig)
        {
            Log newLog = new Log
            {
                IdentificadorId = IdentificadorId,
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters(null, command)
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, QueryEvaluation.TransactionTypes.Insert, useAppConfig, QueryEvaluation.ConnectionTypes.MSSQL, false);
        }
    }
}
