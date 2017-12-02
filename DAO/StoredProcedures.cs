using DataAccess.BO;
using DataAccess.DAO;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DataAccess.DAO
{
    public class StoredProcedures
    {
        #region Enums
        public enum TransactionTypes
        {
            Select,
            SelectAll,
            Delete,
            Insert,
            Update,
            SelectOther
        }
        #endregion

        public static Guid IdentificadorId { get; set; } = Guid.Empty;

        private static MySqlConnection connection;
        private static MySqlCommand command;

        private static void SetParameters<T>(T obj, TransactionTypes transactionType)
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.Name == "DataBaseTableName") continue;

                if (transactionType == TransactionTypes.Delete)
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

        private static void SetParameters(Parameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue(parameters[i].PropertyName, parameters[i].PropertyValue);
            }
        }

        public static Result EjecutarProcedimiento<T>(T obj, string dataBaseTableName, TransactionTypes transactionType, bool logTransaction = true)
        {
            DataTable dataTable = null;

            connection = Connection.OpenConnection();
            command = new MySqlCommand("sp_" + dataBaseTableName + GetFriendlyTransactionType(transactionType), connection);
            command.CommandType = CommandType.StoredProcedure;

            try
            {
                if (transactionType == TransactionTypes.Insert || transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                {
                    SetParameters<T>(obj, transactionType);
                    command.ExecuteNonQuery();
                }
                else
                {
                    if (transactionType == TransactionTypes.Select)
                    {
                        SetParameters<T>(obj, transactionType);
                    }
                    dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                }
            }
            catch (MySqlException mse)
            {
                Debug.WriteLine(mse.Message);
                Connection.CloseConnection(connection);
                return new Result(mse: mse);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                Connection.CloseConnection(connection);
                return new Result(ae: ae);
            }

            Connection.CloseConnection(connection);

            if (logTransaction) LogTransaction(dataBaseTableName, transactionType);

            return new Result(true, dataTable);
        }

        public static Result EjecutarProcedimiento(string dataBaseTableName, string storedProcedure, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

            connection = Connection.OpenConnection();
            command = new MySqlCommand(storedProcedure, connection);
            command.CommandType = CommandType.StoredProcedure;

            try
            {
                if (parameters != null) SetParameters(parameters);
                dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());
            }
            catch (MySqlException mse)
            {
                Debug.WriteLine(mse.Message);
                Connection.CloseConnection(connection);
                return new Result(mse: mse);
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine(ae.Message);
                Connection.CloseConnection(connection);
                return new Result(ae: ae);
            }

            Connection.CloseConnection(connection);

            if (logTransaction) LogTransaction(dataBaseTableName, TransactionTypes.SelectOther);

            return new Result(true, dataTable);
        }

        private static void LogTransaction(string dataBaseTableName, TransactionTypes transactionType)
        {
            Log newLog = new Log();
            string parametros = String.Empty;

            newLog.IdentificadorId = IdentificadorId;
            newLog.Transaccion = transactionType.ToString();
            newLog.TablaAfectada = dataBaseTableName;
            foreach (MySqlParameter parametro in command.Parameters)
            {
                if (parametro.Value != null)
                {
                    parametros += parametro.ParameterName + ": " + parametro.Value + "|";
                }
            }
            newLog.Parametros = parametros;

            EjecutarProcedimiento<Log>(newLog, newLog.DataBaseTableName, TransactionTypes.Insert, false);
        }

        private static string GetFriendlyTransactionType(TransactionTypes transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return "_select";
                case TransactionTypes.Delete:
                    return "_delete";
                case TransactionTypes.Insert:
                    return "_insert";
                case TransactionTypes.Update:
                    return "_update";
                case TransactionTypes.SelectAll:
                    return "_selectAll";
                default:
                    return "_selectAll";
            }
        }
    }
}
