using DataAccess.BO;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataAccess.DAO
{
    public abstract class DbOperation
    {
        protected Guid IdentificadorId { get; set; } = Guid.Empty;
        protected string SelectSuffix { get; set; }
        protected string InsertSuffix { get; set; }
        protected string UpdateSuffix { get; set; }
        protected string DeleteSuffix { get; set; }
        protected string SelectAllSuffix { get; set; }
        protected string StoredProcedurePrefix { get; set; }

        public DbOperation()
        {
            GetTransactionTypesSuffixes();
        }

        private void GetTransactionTypesSuffixes()
        {
            SelectSuffix = ConfigurationManager.AppSettings["SelectSuffix"].ToString();
            InsertSuffix = ConfigurationManager.AppSettings["InsertSuffix"].ToString();
            UpdateSuffix = ConfigurationManager.AppSettings["UpdateSuffix"].ToString();
            DeleteSuffix = ConfigurationManager.AppSettings["DeleteSuffix"].ToString();
            SelectAllSuffix = ConfigurationManager.AppSettings["SelectAllSuffix"].ToString();
            StoredProcedurePrefix = ConfigurationManager.AppSettings["StoredProcedurePrefix"].ToString();
        }

        protected string GetFriendlyTransactionSuffix(QueryEvaluation.TransactionTypes transactionType)
        {
            switch (transactionType)
            {
                case QueryEvaluation.TransactionTypes.Select:
                    return SelectSuffix;
                case QueryEvaluation.TransactionTypes.Delete:
                    return DeleteSuffix;
                case QueryEvaluation.TransactionTypes.Insert:
                    return InsertSuffix;
                case QueryEvaluation.TransactionTypes.Update:
                    return UpdateSuffix;
                case QueryEvaluation.TransactionTypes.SelectAll:
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

        protected void SetParameters<T>(T obj, QueryEvaluation.TransactionTypes transactionType, MySqlCommand mySqlCommand = null, SqlCommand msSqlCommand = null)
        {
            if (msSqlCommand == null && mySqlCommand == null)
            {
                throw new Exception("Se necesita por lo menos un objeto Comando.");
            }

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo entonces se brinca la propiedad.
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(UnlinkedProperty)) != null) continue;

                if (transactionType == QueryEvaluation.TransactionTypes.Delete)
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

            string parametros = String.Empty;

            if (msSqlCommand != null)
            {
                foreach (SqlParameter parametro in msSqlCommand.Parameters)
                {
                    if (parametro.Value != null)
                    {
                        parametros += parametro.ParameterName + ": " + parametro.Value + "|";
                    }
                }
            }
            else
            {
                foreach (MySqlParameter parametro in mySqlCommand.Parameters)
                {
                    if (parametro.Value != null)
                    {
                        parametros += parametro.ParameterName + ": " + parametro.Value + "|";
                    }
                }
            }

            return parametros;
        }

    }
}
