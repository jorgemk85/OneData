using DataAccess.BO;
using System;
using System.Configuration;
using System.Data;

namespace DataAccess.DAO
{
    public abstract class DataManagement<T> where T : new()
    {
        static Result _cache;
        static bool _isPartialCache = false;
        static bool _forceQueryDataBase = false;

        public static Result Insert(T obj, bool useAppConfig, bool isCacheEnabled)
        {
            return Command(obj, QueryEvaluation.TransactionTypes.Insert, useAppConfig, isCacheEnabled);
        }

        public static Result Update(T obj, bool useAppConfig, bool isCacheEnabled)
        {
            return Command(obj, QueryEvaluation.TransactionTypes.Update, useAppConfig, isCacheEnabled);
        }

        public static Result Delete(T obj, bool useAppConfig, bool isCacheEnabled)
        {
            return Command(obj, QueryEvaluation.TransactionTypes.Delete, useAppConfig, isCacheEnabled);
        }

        public static Result Select(bool useAppConfig, bool isCacheEnabled, params Parameter[] parameters)
        {
            return Command(Tools.SetParametersInObject<T>(parameters), QueryEvaluation.TransactionTypes.Select, useAppConfig, isCacheEnabled);
        }

        public static Result Select(string tableName, string storedProcedure, bool useAppConfig, params Parameter[] parameters)
        {
            QueryEvaluation.ConnectionTypes connectionType = (QueryEvaluation.ConnectionTypes)Enum.Parse(typeof(QueryEvaluation.ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());

            if (connectionType == QueryEvaluation.ConnectionTypes.MySQL)
            {
                MySQL mySQL = new MySQL();
                return mySQL.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
            }
            else
            {
                MSSQL msSQL = new MSSQL();
                return msSQL.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
            }
        }

        public static Result SelectAll(bool useAppConfig, bool isCacheEnabled)
        {
            return Command(new T(), QueryEvaluation.TransactionTypes.SelectAll, useAppConfig, isCacheEnabled);
        }

        private static Result Command(T obj, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig, bool isCacheEnabled)
        {
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result resultado;
            if (isCacheEnabled)
            {
                resultado = queryEvaluation.Evaluate(obj, transactionType, _cache, _isPartialCache, _forceQueryDataBase, useAppConfig);
                SaveCache(transactionType, resultado);
            }
            else
            {
                // Al mandar TRUE en forceQueryDataBase aseguramos que no se use el cache y al no almacenar el resultado con la funcion SaveCache, anulamos completamente el uso cache.
                resultado = queryEvaluation.Evaluate(obj, transactionType, _cache, _isPartialCache, true, useAppConfig);
            }

            return resultado;
        }

        private static void SaveCache(QueryEvaluation.TransactionTypes transactionType, Result resultado)
        {
            if (_cache == null || _isPartialCache)
            {
                _forceQueryDataBase = false;
                if (transactionType == QueryEvaluation.TransactionTypes.SelectAll)
                {
                    _cache = resultado;
                    _isPartialCache = false;
                }
                else if (resultado.Data.Rows.Count > 0 && transactionType == QueryEvaluation.TransactionTypes.Select)
                {
                    if (_cache == null)
                    {
                        _cache = resultado;
                    }
                    else
                    {
                        if (!resultado.IsFromCache)
                        {
                            QueryEvaluation queryEvaluation = new QueryEvaluation();
                            foreach (DataRow row in resultado.Data.Rows)
                            {
                                queryEvaluation.AlterCache(row, _cache);
                            }
                        }
                    }

                    _isPartialCache = true;
                }
                else if (transactionType == QueryEvaluation.TransactionTypes.Insert)
                {
                    _forceQueryDataBase = true;
                }
            }
        }
    }
}
