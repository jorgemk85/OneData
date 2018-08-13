using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal class Operation
    {
        protected string SelectSuffix { get; set; }
        protected string InsertSuffix { get; set; }
        protected string UpdateSuffix { get; set; }
        protected string DeleteSuffix { get; set; }
        protected string SelectAllSuffix { get; set; }
        protected string StoredProcedurePrefix { get; set; }

        public Operation()
        {
            GetTransactionTypesSuffixes();
        }

        private void GetTransactionTypesSuffixes()
        {
            string configuration = string.Empty;
            try
            {
                configuration = "SelectSuffix";
                SelectSuffix = ConfigurationManager.AppSettings["SelectSuffix"].ToString();
                configuration = "InsertSuffix";
                InsertSuffix = ConfigurationManager.AppSettings["InsertSuffix"].ToString();
                configuration = "UpdateSuffix";
                UpdateSuffix = ConfigurationManager.AppSettings["UpdateSuffix"].ToString();
                configuration = "DeleteSuffix";
                DeleteSuffix = ConfigurationManager.AppSettings["DeleteSuffix"].ToString();
                configuration = "SelectAllSuffix";
                SelectAllSuffix = ConfigurationManager.AppSettings["SelectAllSuffix"].ToString();
                configuration = "StoredProcedurePrefix";
                StoredProcedurePrefix = ConfigurationManager.AppSettings["StoredProcedurePrefix"].ToString();
            }
            catch (NullReferenceException nre)
            {
                throw new ConfigurationNotFoundException(configuration, nre);
            }
        }

        protected string GetFriendlyTransactionSuffix(TransactionTypes transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return SelectSuffix;
                case TransactionTypes.Delete:
                    return DeleteSuffix;
                case TransactionTypes.Insert:
                    return InsertSuffix;
                case TransactionTypes.Update:
                    return UpdateSuffix;
                case TransactionTypes.SelectAll:
                    return SelectAllSuffix;
                default:
                    return SelectAllSuffix;
            }
        }

        protected void SetParameters(Parameter[] parameters, MySqlCommand mySqlCommand = null, SqlCommand msSqlCommand = null)
        {
            if (msSqlCommand == null && mySqlCommand == null)
            {
                throw new Exception("Se necesita por lo menos un objeto Comando.");
            }

            if (mySqlCommand != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    mySqlCommand.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);
                }
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    msSqlCommand.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);
                }
            }
        }

        protected void SetParameters<T>(T obj, TransactionTypes transactionType, MySqlCommand mySqlCommand = null, SqlCommand msSqlCommand = null)
        {
            if (msSqlCommand == null && mySqlCommand == null)
            {
                throw new Exception("Se necesita por lo menos un objeto Comando.");
            }

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo entonces se brinca la propiedad.
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(UnlinkedProperty)) != null) continue;

                if (transactionType == TransactionTypes.Delete)
                {
                    if (propertyInfo.Name == "Id")
                    {
                        if (mySqlCommand != null)
                        {
                            mySqlCommand.Parameters.AddWithValue("_id", propertyInfo.GetValue(obj));
                            break;
                        }
                        else
                        {
                            msSqlCommand.Parameters.AddWithValue("_id", propertyInfo.GetValue(obj));
                            break;
                        }
                    }
                }
                else
                {
                    if (mySqlCommand != null)
                    {
                        mySqlCommand.Parameters.AddWithValue("_" + propertyInfo.Name, propertyInfo.GetValue(obj));
                    }
                    else
                    {
                        msSqlCommand.Parameters.AddWithValue("_" + propertyInfo.Name, propertyInfo.GetValue(obj));
                    }
                }
            }
        }

        protected string GetStringParameters(MySqlCommand mySqlCommand = null, SqlCommand msSqlCommand = null)
        {
            if (msSqlCommand == null && mySqlCommand == null)
            {
                throw new Exception("Se necesita por lo menos un objeto Comando.");
            }
            StringBuilder builder = new StringBuilder();

            if (msSqlCommand != null)
            {
                foreach (SqlParameter parametro in msSqlCommand.Parameters)
                {
                    if (parametro.Value != null)
                    {
                        builder.AppendFormat("{0}: {1}|", parametro.ParameterName, parametro.Value);
                    }
                }
            }
            else
            {
                foreach (MySqlParameter parametro in mySqlCommand.Parameters)
                {
                    if (parametro.Value != null)
                    {
                        builder.AppendFormat("{0}: {1}|", parametro.ParameterName, parametro.Value);
                    }
                }
            }

            return builder.ToString();
        }

        public virtual Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            return new Result();
        }

        public virtual Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new()
        {
            return new Result();
        }

        internal virtual void ExecuteNonQuery(string query, string connectionToUse) { }

        protected string GetTransactionText<T>(TransactionTypes transactionType, ConnectionTypes connectionType) where T : IManageable, new()
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Creation.CreateSelectStoredProcedure<T>(connectionType);
                case TransactionTypes.SelectAll:
                    return Creation.CreateSelectAllStoredProcedure<T>(connectionType);
                case TransactionTypes.Delete:
                    return Creation.CreateDeleteStoredProcedure<T>(connectionType);
                case TransactionTypes.Insert:
                    return Creation.CreateInsertStoredProcedure<T>(connectionType);
                case TransactionTypes.Update:
                    return Creation.CreateUpdateStoredProcedure<T>(connectionType);
                default:
                    throw new ArgumentException("El tipo de trascaccion no es valido para generar un nuevo procedimiento almacenado.");
            }
        }
    }
}
