﻿using DataAccess.BO;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataAccess.DAO
{
    public class MSSQL : DataConnectionType
    {
        private SqlConnection connection;
        private SqlCommand command;

        private void SetParameters<T>(T obj, QueryEvaluation.TransactionTypes transactionType)
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo entonces se brinca la propiedad.
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(UnlinkedProperty)) != null) continue;

                if (transactionType == QueryEvaluation.TransactionTypes.Delete)
                {
                    if (propertyInfo.Name == "Id")
                    {
                        command.Parameters.AddWithValue("_id", propertyInfo.GetValue(obj));
                        break;
                    }
                }
                else
                {
                    command.Parameters.AddWithValue("_" + propertyInfo.Name, propertyInfo.GetValue(obj));
                }
            }
        }

        private void SetParameters(Parameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);
            }
        }

        public Result EjecutarProcedimiento<T>(T obj, string tableName, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig, bool logTransaction = true)
        {
            DataTable dataTable = null;

            connection = Connection.OpenMSSQLConnection(useAppConfig);
            if (connection.State != ConnectionState.Open) return new Result(exito: false, mensaje: "No se puede abrir la conexion con la base de datos.", titulo: "Error al intentar conectar.");
            command = new SqlCommand(string.Format("{0}sp_{1}{2}", (obj as Main).Schema + ".", tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
            command.CommandType = CommandType.StoredProcedure;

            try
            {
                if (transactionType == QueryEvaluation.TransactionTypes.Insert || transactionType == QueryEvaluation.TransactionTypes.Update || transactionType == QueryEvaluation.TransactionTypes.Delete)
                {
                    SetParameters(obj, transactionType);
                    command.ExecuteNonQuery();
                }
                else
                {
                    if (transactionType == QueryEvaluation.TransactionTypes.Select)
                    {
                        SetParameters(obj, transactionType);
                    }
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (SqlException mssqle)
            {
                Debug.WriteLine(mssqle.Message);
                Connection.CloseConnection(connection);
                return new Result(mssqle: mssqle);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                Connection.CloseConnection(connection);
                return new Result(ae: ae);
            }

            Connection.CloseConnection(connection);

            if (logTransaction) LogTransaction(tableName, transactionType, useAppConfig);

            return new Result(true, dataTable, Tools.MSSqlParameterCollectionToList(command.Parameters));
        }

        public Result EjecutarProcedimiento(string tableName, string storedProcedure, Parameter[] parameters, bool useAppConfig, bool logTransaction = true)
        {
            DataTable dataTable = null;

            connection = Connection.OpenMSSQLConnection(useAppConfig);
            if (connection.State != ConnectionState.Open) return new Result(exito: false, mensaje: "No se puede abrir la conexion con la base de datos.", titulo: "Error al intentar conectar.");
            command = new SqlCommand(storedProcedure, connection);
            command.CommandType = CommandType.StoredProcedure;

            try
            {
                if (parameters != null) SetParameters(parameters);
                dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());
                dataTable.TableName = tableName;
            }
            catch (SqlException mssqle)
            {
                Debug.WriteLine(mssqle.Message);
                Connection.CloseConnection(connection);
                return new Result(mssqle: mssqle);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                Connection.CloseConnection(connection);
                return new Result(ae: ae);
            }

            Connection.CloseConnection(connection);

            if (logTransaction) LogTransaction(tableName, QueryEvaluation.TransactionTypes.SelectOther, useAppConfig);

            return new Result(true, dataTable);
        }

        private void LogTransaction(string dataBaseTableName, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig)
        {
            Log newLog = new Log();
            string parametros = String.Empty;

            newLog.IdentificadorId = IdentificadorId;
            newLog.Transaccion = transactionType.ToString();
            newLog.TablaAfectada = dataBaseTableName;
            foreach (SqlParameter parametro in command.Parameters)
            {
                if (parametro.Value != null)
                {
                    parametros += parametro.ParameterName + ": " + parametro.Value + "|";
                }
            }
            newLog.Parametros = parametros;

            EjecutarProcedimiento(newLog, newLog.DataBaseTableName, QueryEvaluation.TransactionTypes.Insert, useAppConfig, false);
        }
    }
}
