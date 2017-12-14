using System;
using System.Collections.Generic;
using System.Data;
using DataAccess.BO;

namespace DataAccess.DAO
{
    public abstract class DataAccess<T> : IConnectable where T : new()
    {
        static Main main = new T() as Main;
        static Result resultado, cache;
        static bool isPartialCache = false;
        static bool forceQueryDataBase = false;

        protected static Result Insert(T obj) => Command(obj, StoredProcedures.TransactionTypes.Insert);

        protected static Result Update(T obj) => Command(obj, StoredProcedures.TransactionTypes.Update);

        protected static Result Delete(T obj) => Command(obj, StoredProcedures.TransactionTypes.Delete);

        protected static Result Select(params Parameter[] parameters) => Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select);

        protected static Result Select(string tableName, string storedProcedure, params Parameter[] parameters) => StoredProcedures.EjecutarProcedimiento(tableName, storedProcedure, parameters);

        protected static Result SelectAll() => Command(new T(), StoredProcedures.TransactionTypes.SelectAll);

        private static Result Command(T obj, StoredProcedures.TransactionTypes transactionType)
        {
            resultado = CacheEvaluation.Evaluate(obj, transactionType, cache, isPartialCache, forceQueryDataBase);
            SaveCache(transactionType);
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
                                CacheEvaluation.AlterCache(row, cache);
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

        #region Abstraction for Interface
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
