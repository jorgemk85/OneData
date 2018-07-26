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
        static bool _isCacheEnabled;
        static long _cacheValidity;
        static long _lastCacheUpdate;

        static DataManagement()
        {
            _isCacheEnabled = bool.Parse(ConfigurationManager.AppSettings["IsCacheEnabled"].ToString());
            _cacheValidity = long.Parse(ConfigurationManager.AppSettings["CacheValidity"].ToString()) * TimeSpan.TicksPerSecond;
            _lastCacheUpdate = DateTime.Now.Ticks;
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

            if (connectionType == QueryEvaluation.ConnectionTypes.MySQL)
            {
                MySqlOperation mySQL = new MySqlOperation();
                return mySQL.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
            }
            else
            {
                MsSqlOperation msSQL = new MsSqlOperation();
                return msSQL.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
            }
        }

        public static Result SelectAll(bool useAppConfig)
        {
            return Command(new T(), QueryEvaluation.TransactionTypes.SelectAll, useAppConfig);
        }

        private static Result Command(T obj, QueryEvaluation.TransactionTypes transactionType, bool useAppConfig)
        {
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result resultado;
            if (_isCacheEnabled)
            {
                RenewCache();
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
                // Cada vez que actualizamos el cache se debe de actualizar la variable para determinar cuando fue la ultima vez que se actualizo el cache
                _lastCacheUpdate = DateTime.Now.Ticks;

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

        private static void RenewCache()
        {
            if (DateTime.Now.Ticks > _lastCacheUpdate + _cacheValidity)
            {
                // Elimina el cache ya que esta EXPIRADO y de debe de refrescar.
                _cache = null;
            }
        }
    }
}
