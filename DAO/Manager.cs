using DataManagement.BO;
using System;
using System.Configuration;
using System.Data;

namespace DataManagement.DAO
{
    public abstract class Manager<T> where T : new()
    {
        static DataCache dataCache = new DataCache();
        static bool forceQueryDataBase = false;

        static Manager()
        {
            dataCache.Initialize(new T());
        }

        public static Result Insert(T obj, bool useAppConfig)
        {
            return Command(obj, DbOperation.TransactionTypes.Insert, useAppConfig);
        }

        public static Result Update(T obj, bool useAppConfig)
        {
            return Command(obj, DbOperation.TransactionTypes.Update, useAppConfig);
        }

        public static Result Delete(T obj, bool useAppConfig)
        {
            return Command(obj, DbOperation.TransactionTypes.Delete, useAppConfig);
        }

        public static Result Select(bool useAppConfig, params Parameter[] parameters)
        {
            return Command(Tools.SetParametersInObject<T>(parameters), DbOperation.TransactionTypes.Select, useAppConfig);
        }

        public static Result Select(string tableName, string storedProcedure, bool useAppConfig, params Parameter[] parameters)
        {
            DbOperation.ConnectionTypes connectionType = (DbOperation.ConnectionTypes)Enum.Parse(typeof(DbOperation.ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());

            DbOperation dbOperation = connectionType == DbOperation.ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();

            return dbOperation.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
        }

        public static Result SelectAll(bool useAppConfig)
        {
            return Command(new T(), DbOperation.TransactionTypes.SelectAll, useAppConfig);
        }

        private static Result Command(T obj, DbOperation.TransactionTypes transactionType, bool useAppConfig)
        {
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result resultado;
            if (dataCache.IsCacheEnabled)
            {
                ResetCacheIfExpired();
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

        private static void SaveCache(DbOperation.TransactionTypes transactionType, Result resultado)
        {
            if (dataCache.Cache == null || dataCache.IsPartialCache)
            {
                // Cada vez que actualizamos el cache se debe de actualizar la variable para determinar cuando fue la ultima vez que se actualizo el cache
                dataCache.LastCacheUpdate = DateTime.Now.Ticks;

                forceQueryDataBase = false;

                if (transactionType == DbOperation.TransactionTypes.SelectAll)
                {
                    dataCache.Cache = resultado;
                    dataCache.IsPartialCache = false;
                }
                else if (resultado.Data.Rows.Count > 0 && transactionType == DbOperation.TransactionTypes.Select)
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
                else if (transactionType == DbOperation.TransactionTypes.Insert)
                {
                    forceQueryDataBase = true;
                }
            }
        }

        private static void ResetCacheIfExpired()
        {
            if (DateTime.Now.Ticks > dataCache.LastCacheUpdate + dataCache.CacheExpiration)
            {
                // Elimina el cache ya que esta EXPIRADO y de debe de refrescar.
                dataCache.Reset(new T());
            }
        }
    }
}
