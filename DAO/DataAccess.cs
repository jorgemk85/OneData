using System;
using System.Collections.Generic;
using System.Data;
using DataAccess.BO;

namespace DataAccess.DAO
{
    public abstract class DataAccess<T> : IConnectable where T : new()
    {
        public static bool IsCacheEnabled { get; set; } = true;

        static Main main = new T() as Main;
        static Result resultado, cache;
        static bool isPartialCache = false;
        static bool forceQueryDataBase = false;

        protected static Result Insert(T obj, bool useAppConfig) => Command(obj, StoredProcedures.TransactionTypes.Insert, useAppConfig);

        protected static Result Update(T obj, bool useAppConfig) => Command(obj, StoredProcedures.TransactionTypes.Update, useAppConfig);

        protected static Result Delete(T obj, bool useAppConfig) => Command(obj, StoredProcedures.TransactionTypes.Delete, useAppConfig);

        protected static Result Select(bool useAppConfig, params Parameter[] parameters) => Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select, useAppConfig);

        protected static Result Select(string tableName, string storedProcedure, bool useAppConfig, params Parameter[] parameters) => StoredProcedures.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);

        protected static Result SelectAll(bool useAppConfig) => Command(new T(), StoredProcedures.TransactionTypes.SelectAll, useAppConfig);

        private static Result Command(T obj, StoredProcedures.TransactionTypes transactionType, bool useAppConfig)
        {
            if (IsCacheEnabled)
            {
                resultado = QueryEvaluation.Evaluate(obj, transactionType, cache, isPartialCache, forceQueryDataBase, useAppConfig);
                SaveCache(transactionType);
            }
            else
            {
                // Al mandar TRUE en forceQueryDataBase aseguramos que no se use el cache y al no almacenarlo con la funcion SaveCache, anulamos completamente el uso cache.
                resultado = QueryEvaluation.Evaluate(obj, transactionType, cache, isPartialCache, true, useAppConfig);
            }

            return resultado;
        }

        private static void SaveCache(StoredProcedures.TransactionTypes transactionType)
        {
            if (cache == null || isPartialCache)
            {
                forceQueryDataBase = false;
                if (transactionType == StoredProcedures.TransactionTypes.SelectAll)
                {
                    cache = resultado;
                    isPartialCache = false;
                }
                else if (resultado.Data.Rows.Count > 0 && transactionType == StoredProcedures.TransactionTypes.Select)
                {
                    if (cache == null)
                    {
                        cache = resultado;
                    }
                    else
                    {
                        if (!resultado.IsFromCache)
                        {
                            foreach (DataRow row in resultado.Data.Rows)
                            {
                                QueryEvaluation.AlterCache(row, cache);
                            }
                        }
                    }

                    isPartialCache = true;
                }
                else if (transactionType == StoredProcedures.TransactionTypes.Insert)
                {
                    forceQueryDataBase = true;
                }
            }
        }

        #region Abstraction for Interface and Inheritance
        public abstract Result Insert<I>(I obj) where I : new();
        public abstract Result Update<I>(I obj) where I : new();
        public abstract Result Delete<I>(I obj) where I : new();
        public abstract I Select<I>(params Parameter[] parameters) where I : new();
        public abstract List<I> SelectList<I>(params Parameter[] parameters) where I : new();
        public abstract Dictionary<Guid, I> SelectDictionary<I>(params Parameter[] parameters) where I : new();
        public abstract Result SelectOther(string tableName, string storedProcedure, params Parameter[] parameters);
        public abstract List<I> SelectAllList<I>() where I : new();
        public abstract Dictionary<Guid, I> SelectAllDictionary<I>() where I : new();
        #endregion
    }
}
