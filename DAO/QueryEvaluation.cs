using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace DataAccess.DAO
{
    public static class QueryEvaluation
    {
        public static Result Evaluate<T>(T obj, StoredProcedures.TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, bool useAppConfig) where T : new()
        {
            string tableName = (obj as Main).DataBaseTableName;
            Result resultado = new Result();
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case StoredProcedures.TransactionTypes.Select:
                    EvaluateSelect(obj, out resultado, isCached, tableName, cache, isPartialCache, forceQueryDataBase, useAppConfig);
                    break;
                case StoredProcedures.TransactionTypes.SelectAll:
                    EvaluateSelectAll(obj, out resultado, isCached, tableName, cache, forceQueryDataBase, useAppConfig);
                    break;
                case StoredProcedures.TransactionTypes.Delete:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType, useAppConfig);
                    if (isCached && resultado.TuvoExito) DeleteInCache(obj, cache);
                    break;
                case StoredProcedures.TransactionTypes.Insert:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType, useAppConfig);
                    if (isCached && resultado.TuvoExito) InsertInCache(obj, cache);
                    break;
                case StoredProcedures.TransactionTypes.Update:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType, useAppConfig);
                    if (isCached && resultado.TuvoExito) UpdateInCache(obj, cache);
                    break;
                case StoredProcedures.TransactionTypes.SelectOther:
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, transactionType, useAppConfig);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        private static void EvaluateSelect<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool isPartialCache, bool forceQueryDataBase, bool useAppConfig) where T : new()
        {
            if (forceQueryDataBase)
            {
                resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, StoredProcedures.TransactionTypes.Select, useAppConfig);
            }
            else
            {
                resultado = isCached == true ? SelectInCache(obj, cache) : StoredProcedures.EjecutarProcedimiento(obj, tableName, StoredProcedures.TransactionTypes.Select, useAppConfig);
                resultado.IsFromCache = isCached == true ? true : false;
                if (isCached && isPartialCache && resultado.TuvoExito && resultado.Data.Rows.Count == 0)
                {
                    resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, StoredProcedures.TransactionTypes.Select, useAppConfig);
                }
            }
        }

        private static void EvaluateSelectAll<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool forceQueryDataBase, bool useAppConfig)
        {
            if (forceQueryDataBase)
            {
                resultado = StoredProcedures.EjecutarProcedimiento(obj, tableName, StoredProcedures.TransactionTypes.SelectAll, useAppConfig);
            }
            else
            {
                resultado = isCached == true ? cache : StoredProcedures.EjecutarProcedimiento(obj, tableName, StoredProcedures.TransactionTypes.SelectAll, useAppConfig);
                resultado.IsFromCache = isCached == true ? true : false;
            }
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
                        predicate += prop.Name + "== @" + valueIndex + " and ";
                        values.Add(prop.GetValue(obj));
                        valueIndex++;
                    }
                }
            }

            if (string.IsNullOrEmpty(predicate))
            {
                return new Result(exito: false, titulo: "Error en los parametros.", mensaje: "Al parecer no se establecieron paramentros para la busqueda o no se obtuvieron resultados previos a la consulta. Por favor asegurece de por lo menos colocar uno valido y volver a intentar.");
            }
            else
            {
                predicate = predicate.Substring(0, predicate.Length - 5);
                return new Result(exito: true, data: Tools.ConvertListToDataTableOfType(Tools.ConvertDataTableToListOfType<T>(cache.Data).Where(predicate, values.ToArray()).ToList()));
            }
        }

        private static DataRow SetRowData<T>(DataRow row, T obj, bool isInsert)
        {
            object value = null;
            Type type;

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (row.Table.Columns.Contains(prop.Name))
                {
                    value = prop.GetValue(obj);
                    // La base de datos no acepta nulls, entonces verifica si es para asignarle un valor
                    if (value == null)
                    {
                        type = Nullable.GetUnderlyingType(prop.PropertyType) != null ? type = Nullable.GetUnderlyingType(prop.PropertyType) : type = prop.PropertyType;

                        switch (type.Name)
                        {
                            case "String":
                                value = string.Empty;
                                break;
                            default:
                                value = Activator.CreateInstance(type);
                                break;
                        }
                    }
                    row[prop.Name] = value;
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
            cache.Data.AcceptChanges();
        }

        public static void AlterCache(DataRow row, Result cache)
        {
            DataRow cacheRow = cache.Data.Rows.Find(row[row.Table.PrimaryKey[0]]);
            string columnName = string.Empty;

            if (cacheRow == null)
            {
                // NO existe la fila: la agrega.
                cache.Data.Rows.Add(row.ItemArray);
                cache.Data.AcceptChanges();
            }
            else
            {
                // SI existe la fila: la actualiza.
                for (int i = 0; i < cacheRow.ItemArray.Length; i++)
                {
                    columnName = cacheRow.Table.Columns[i].ColumnName;
                    cacheRow[columnName] = row[columnName];
                }
            }
            cache.Data.AcceptChanges();
        }

        private static void DeleteInCache<T>(T obj, Result cache)
        {
            for (int i = 0; i < cache.Data.Rows.Count; i++)
            {
                DataRow row = cache.Data.Rows[i];
                if (row[row.Table.PrimaryKey[0]].Equals((obj as Main).Id.GetValueOrDefault()))
                {
                    row.Delete();
                    cache.Data.AcceptChanges();
                    break;
                }
            }
        }
    }
}
