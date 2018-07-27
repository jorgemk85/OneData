using DataAccess.BO;
using System;
using System.Configuration;
using System.Data;

namespace DataAccess.DAO
{
    public abstract class DataManagement<T> where T : new()
    {
        static DataCache dataCache = new DataCache();
        static bool forceQueryDataBase = false;

        static DataManagement()
        {
            T mainObj = new T();

            dataCache.IsCacheEnabled = (mainObj as Main).IsCacheEnabled;
            dataCache.CacheExpiration = long.Parse((mainObj as Main).CacheExpiration.ToString()) * TimeSpan.TicksPerSecond;
            dataCache.LastCacheUpdate = DateTime.Now.Ticks;
        }

        public static Result Insert(T obj, bool useAppConfig)
        {
            return Command(obj, QueryEvaluation.TransactionTypes.Insert, useAppConfig);
        }

        public static Result Update(T obj, bool useAppConfig)
        {
            return Command(obj, QueryEvaluation.TransactionTypes.Update, useAppConfig);
        }

        public static Result Delete(T obj, bool useAppConfig)
        {
            return Command(obj, QueryEvaluation.TransactionTypes.Delete, useAppConfig);
        }

        public static Result Select(bool useAppConfig, params Parameter[] parameters)
        {
            return Command(Tools.SetParametersInObject<T>(parameters), QueryEvaluation.TransactionTypes.Select, useAppConfig);
        }

        public static Result Select(string tableName, string storedProcedure, bool useAppConfig, params Parameter[] parameters)
        {
            QueryEvaluation.ConnectionTypes connectionType = (QueryEvaluation.ConnectionTypes)Enum.Parse(typeof(QueryEvaluation.ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());

            DbOperation dbOperation = connectionType == QueryEvaluation.ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();

            return dbOperation.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
        }

        public static Result SelectAll(bool useAppConfig)
        {
            return Command(new T(), QueryEvaluation.TransactionTypes.SelectAll, useAppConfig);
        }

        private static Result Command(T obj, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig)
        {
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result resultado;
            if (dataCache.IsCacheEnabled)
            {
                DeleteCacheIfExpired();
                resultado = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, forceQueryDataBase, useAppConfig);
                SaveCache(transactionType, resultado);
            }
            else
            {
                // Al mandar TRUE en forceQueryDataBase aseguramos que no se use el cache y al no almacenar el resultado con la funcion SaveCache, anulamos completamente el uso cache.
                resultado = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, true, useAppConfig);
            }

            return resultado;
        }

        private static void SaveCache(QueryEvaluation.TransactionTypes transactionType, Result resultado)
        {
            if (dataCache.Cache == null || dataCache.IsPartialCache)
            {
                // Cada vez que actualizamos el cache se debe de actualizar la variable para determinar cuando fue la ultima vez que se actualizo el cache
                dataCache.LastCacheUpdate = DateTime.Now.Ticks;

                forceQueryDataBase = false;

                if (transactionType == QueryEvaluation.TransactionTypes.SelectAll)
                {
                    dataCache.Cache = resultado;
                    dataCache.IsPartialCache = false;
                }
                else if (resultado.Data.Rows.Count > 0 && transactionType == QueryEvaluation.TransactionTypes.Select)
                {
                    if (dataCache.Cache == null)
                    {
                        dataCache.Cache = resultado;
                    }
                    else
                    {
                        if (!resultado.IsFromCache)
                        {
                            QueryEvaluation queryEvaluation = new QueryEvaluation();
                            foreach (DataRow row in resultado.Data.Rows)
                            {
                                queryEvaluation.AlterCache(row, dataCache.Cache);
                            }
                        }
                    }

                    dataCache.IsPartialCache = true;
                }
                else if (transactionType == QueryEvaluation.TransactionTypes.Insert)
                {
                    forceQueryDataBase = true;
                }
            }
        }

        private static void DeleteCacheIfExpired()
        {
            if (DateTime.Now.Ticks > dataCache.LastCacheUpdate + dataCache.CacheExpiration)
            {
                // Elimina el cache ya que esta EXPIRADO y de debe de refrescar.
                dataCache.Cache = null;
            }
        }
    }
}
