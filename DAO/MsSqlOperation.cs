using DataManagement.Enums;
using DataManagement.Events;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Data;
using System.Data.SqlClient;

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
                using (SqlConnection connection = Connection.OpenMsSqlConnection(useAppConfig))
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

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, useAppConfig);

            return new Result(dataTable);
        }

        public override Result ExecuteProcedure<T>(T obj, string tableName, TransactionTypes transactionType, bool useAppConfig, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                dataTable = ConfigureConnectionAndExecuteCommand<T>(obj, tableName, transactionType, useAppConfig);
            }
            catch (SqlException mssqle)
            {
                throw mssqle;
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }

            if (logTransaction) LogTransaction(tableName, transactionType, useAppConfig);

            return new Result(dataTable);
        }

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, TransactionTypes transactionType, bool useAppConfig)
        {
            DataTable dataTable = null;

            using (SqlConnection connection = Connection.OpenMsSqlConnection(useAppConfig))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
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
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters(null, command)
            };

            ExecuteProcedure<Log>(newLog, newLog.DataBaseTableName, TransactionTypes.Insert, useAppConfig, false);
        }
    }
}
