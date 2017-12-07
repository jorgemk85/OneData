using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public static class CacheEvaluation
    {
        public static Result Evaluate<T>(T obj, StoredProcedures.TransactionTypes transactionType, Result cache) where T : new()
        {
            string tableName = (obj as Main).DataBaseTableName;
            Result resultado = new Result();
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case StoredProcedures.TransactionTypes.Select:
                    resultado = isCached == true ? SelectInCache(obj, cache) : StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType);
                    break;
                case StoredProcedures.TransactionTypes.SelectAll:
                    resultado = isCached == true ? cache : StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType);
                    if (!isCached) cache = resultado;
                    break;
                case StoredProcedures.TransactionTypes.Delete:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType);
                    if (isCached && resultado.TuvoExito) DeleteInCache(obj, cache);
                    break;
                case StoredProcedures.TransactionTypes.Insert:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType);
                    if (isCached && resultado.TuvoExito) InsertInCache(obj, cache);
                    break;
                case StoredProcedures.TransactionTypes.Update:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType);
                    if (isCached && resultado.TuvoExito) UpdateInCache(obj, cache);
                    break;
                case StoredProcedures.TransactionTypes.SelectOther:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        private static Result SelectInCache<T>(T obj, Result cache) where T : new()
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

        private static DataRow SetRowData<T>(DataRow row, T obj, bool isInsert)
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

        private static void UpdateInCache<T>(T obj, Result cache)
        {
            SetRowData<T>(cache.Data.Rows.Find((obj as Main).Id), obj, false).AcceptChanges();
        }

        private static void InsertInCache<T>(T obj, Result cache)
        {
            (obj as Main).FechaCreacion = DateTime.Now;
            (obj as Main).FechaModificacion = DateTime.Now;

            cache.Data.Rows.Add(SetRowData(cache.Data.NewRow(), obj, true));
        }

        private static void DeleteInCache<T>(T obj, Result cache)
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
    }
}
