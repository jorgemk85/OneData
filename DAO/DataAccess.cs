using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataAccess.DAO
{
    public abstract class DataAccess<T> where T : new()
    {
        static Main main = new T() as Main;
        static Result resultado;
        static Result cache;

        private static Result Command(T obj, StoredProcedures.TransactionTypes transactionType)
        {
            resultado = CacheEvaluation.Evaluate(obj, transactionType, cache);

            if (cache == null && transactionType == StoredProcedures.TransactionTypes.SelectAll) cache = resultado;
            if (!resultado.TuvoExito) MessageBox.Show(resultado.Mensaje, resultado.TituloMensaje, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return resultado;
        }

        public static Result Insert(T obj) => Command(obj, StoredProcedures.TransactionTypes.Insert);

        public static Result Update(T obj) => Command(obj, StoredProcedures.TransactionTypes.Update);

        public static Result Delete(T obj) => Command(obj, StoredProcedures.TransactionTypes.Delete);

        public static List<T> SelectAllList()
        {
            Command(new T(), StoredProcedures.TransactionTypes.SelectAll);

            return resultado.TuvoExito ? Tools.ConvertDataTableToListOfType<T>(resultado.Data) : new List<T>();
        }

        public static Dictionary<Guid, T> SelectAllDictionary()
        {
            Command(new T(), StoredProcedures.TransactionTypes.SelectAll);

            return resultado.TuvoExito ? Tools.ConvertDataTableToDictionaryOfType<T>(resultado.Data) : new Dictionary<Guid, T>();
        }

        public static T Select(params Parameter[] parameters)
        {
            Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select);

            return resultado.TuvoExito ? Tools.ConvertDataTableToObjectOfType<T>(resultado.Data) : new T();
        }

        public static List<T> SelectList(params Parameter[] parameters)
        {
            Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select);

            return resultado.TuvoExito ? Tools.ConvertDataTableToListOfType<T>(resultado.Data) : new List<T>();
        }

        public static Dictionary<Guid, T> SelectDictionary(params Parameter[] parameters)
        {
            Command(Tools.SetParametersInObject<T>(parameters), StoredProcedures.TransactionTypes.Select);

            return resultado.TuvoExito ? Tools.ConvertDataTableToDictionaryOfType<T>(resultado.Data) : new Dictionary<Guid, T>();
        }

        public static Result SelectOther(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            resultado = StoredProcedures.EjecutarProcedimiento(tableName, storedProcedure, parameters);
            if (!resultado.TuvoExito) MessageBox.Show(resultado.Mensaje, resultado.TituloMensaje, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return resultado;
        }
    }
}
