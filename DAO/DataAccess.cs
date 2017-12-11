using DataAccess.BO;
using System;
using System.Collections.Generic;

namespace DataAccess.DAO
{
    public abstract class DataAccess<T> where T : new()
    {
        static Main main = new T() as Main;
        static Result resultado, cache;

        private static Result Command(T obj, StoredProcedures.TransactionTypes transactionType)
        {
            resultado = CacheEvaluation.Evaluate(obj, transactionType, cache);

            if (cache == null && transactionType == StoredProcedures.TransactionTypes.SelectAll) cache = resultado;

            return resultado;
        }

        public static Result Insert(T obj) => Command(obj, StoredProcedures.TransactionTypes.Insert);

        public static Result Update(T obj) => Command(obj, StoredProcedures.TransactionTypes.Update);

        public static Result Delete(T obj) => Command(obj, StoredProcedures.TransactionTypes.Delete);

        public static T Select(params Parameter[] parameters)
        {
            return Tools.ConvertDataTableToObjectOfType<T>(Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select).Data);
        }

        public static List<T> SelectList(params Parameter[] parameters)
        {
            return Tools.ConvertDataTableToListOfType<T>(Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select).Data);
        }

        public static Dictionary<Guid, T> SelectDictionary(params Parameter[] parameters)
        {
           return Tools.ConvertDataTableToDictionaryOfType<T>(Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select).Data);
        }

        public static Result SelectOther(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return StoredProcedures.EjecutarProcedimiento(tableName, storedProcedure, parameters);
        }

        public static List<T> SelectAllList()
        {
            return Tools.ConvertDataTableToListOfType<T>(Command(new T(), StoredProcedures.TransactionTypes.SelectAll).Data);
        }

        public static Dictionary<Guid, T> SelectAllDictionary()
        {
            return Tools.ConvertDataTableToDictionaryOfType<T>(Command(new T(), StoredProcedures.TransactionTypes.SelectAll).Data);
        }
    }
}
