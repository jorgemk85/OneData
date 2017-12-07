using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
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
            EvaluateCache(obj, transactionType);

            if (!resultado.TuvoExito) MessageBox.Show(resultado.Mensaje, resultado.TituloMensaje, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return resultado;
        }

        private static void EvaluateCache(T obj, StoredProcedures.TransactionTypes transactionType)
        {
            Type type = typeof(T);
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case StoredProcedures.TransactionTypes.Select:
                    resultado = isCached == true ? SelectInCache(obj) : StoredProcedures.EjecutarProcedimiento<T>(obj, main.DataBaseTableName, transactionType);
                    break;
                case StoredProcedures.TransactionTypes.SelectAll:
                    resultado = isCached == true ? cache : StoredProcedures.EjecutarProcedimiento<T>(obj, main.DataBaseTableName, transactionType);
                    if (!isCached) cache = resultado;
                    break;
                case StoredProcedures.TransactionTypes.Delete:
                    resultado = StoredProcedures.EjecutarProcedimiento<T>(obj, main.DataBaseTableName, transactionType);
                    if (isCached && resultado.TuvoExito) DeleteInCache(obj);
                    break;
                case StoredProcedures.TransactionTypes.Insert:
                    resultado = StoredProcedures.EjecutarProcedimiento<T>(obj, main.DataBaseTableName, transactionType);
                    if (isCached && resultado.TuvoExito) InsertInCache(obj);
                    break;
                case StoredProcedures.TransactionTypes.Update:
                    resultado = StoredProcedures.EjecutarProcedimiento<T>(obj, main.DataBaseTableName, transactionType);
                    if (isCached && resultado.TuvoExito) UpdateInCache(obj);
                    break;
                case StoredProcedures.TransactionTypes.SelectOther:
                    resultado = StoredProcedures.EjecutarProcedimiento<T>(obj, main.DataBaseTableName, transactionType);
                    break;
                default:
                    break;
            }
        }

        private static Result SelectInCache(T obj)
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(UnlinkedProperty)) == null)
                {
                    if (prop.GetValue(obj) != null)
                    {
                        predicate += prop.Name + "== @" + valueIndex;
                        values.Add(prop.GetValue(obj));
                        valueIndex++;
                    }
                }
            }

            return new Result(exito: true, data: Tools.ConvertListToDataTableOfType<T>(Tools.ConvertDataTableToListOfType<T>(cache.Data).Where<T>(predicate, values.ToArray()).ToList()));
        }

        private static DataRow SetRowData(DataRow row, T obj, bool isInsert)
        {
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(UnlinkedProperty)) == null)
                {
                    if ((prop.Name != "FechaCreacion" && prop.Name != "FechaModificacion") || isInsert)
                    {
                        row[prop.Name] = prop.GetValue(obj);
                    }
                }
            }
            return row;
        }

        private static void UpdateInCache(T obj)
        {
            SetRowData(cache.Data.Rows.Find((obj as Main).Id), obj, false).AcceptChanges();
        }

        private static void InsertInCache(T obj)
        {
            (obj as Main).FechaCreacion = DateTime.Now;
            (obj as Main).FechaModificacion = DateTime.Now;

            cache.Data.Rows.Add(SetRowData(cache.Data.NewRow(), obj, true));
        }

        private static void DeleteInCache(T obj)
        {
            DataRow row;

            for (int i = 0; i < cache.Data.Rows.Count; i++)
            {
                row = cache.Data.Rows[i];
                if (row["Id"].Equals((obj as Main).Id.GetValueOrDefault()))
                {
                    row.Delete();
                    cache.Data.AcceptChanges();
                    break;
                }
            }
        }

        public static Result Insert(T obj)
        {
            return Command(obj, StoredProcedures.TransactionTypes.Insert);
        }

        public static Result Update(T obj)
        {
            return Command(obj, StoredProcedures.TransactionTypes.Update);
        }

        public static Result Delete(T obj)
        {
            return Command(obj, StoredProcedures.TransactionTypes.Delete);
        }

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
            Command(SetValuesInObject(parameters), StoredProcedures.TransactionTypes.Select);

            return resultado.TuvoExito ? Tools.ConvertDataTableToObjectOfType<T>(resultado.Data) : new T();
        }

        public static List<T> SelectList(params Parameter[] parameters)
        {
            Command(SetValuesInObject(parameters), StoredProcedures.TransactionTypes.Select);

            return resultado.TuvoExito ? Tools.ConvertDataTableToListOfType<T>(resultado.Data) : new List<T>();
        }

        public static Dictionary<Guid, T> SelectDictionary(params Parameter[] parameters)
        {
            Command(SetValuesInObject(parameters), StoredProcedures.TransactionTypes.Select);

            return resultado.TuvoExito ? Tools.ConvertDataTableToDictionaryOfType<T>(resultado.Data) : new Dictionary<Guid, T>();
        }

        public static Result SelectOther(string dataBaseTableName, string storedProcedure, params Parameter[] parameters)
        {
            resultado = StoredProcedures.EjecutarProcedimiento(dataBaseTableName, storedProcedure, parameters);
            if (!resultado.TuvoExito) MessageBox.Show(resultado.Mensaje, resultado.TituloMensaje, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return resultado;
        }

        private static T SetValuesInObject(Parameter[] parameters)
        {
            PropertyInfo propertyInfo;
            T newObj = new T();

            (newObj as Main).Id = null;

            foreach (Parameter data in parameters)
            {
                propertyInfo = typeof(T).GetProperty(data.PropertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(newObj, data.PropertyValue);
                }
            }

            return newObj;
        }
    }
}
