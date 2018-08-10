using DataManagement.Attributes;
using DataManagement.Models;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal class QueryEvaluation
    {
        DbOperation dbOperation;
        ConnectionTypes connectionType;

        public QueryEvaluation()
        {
            connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
            dbOperation = connectionType == ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();
        }

        public Result Evaluate<T>(T obj, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, bool useAppConfig) where T : new()
        {
            string tableName = (obj as Main).DataBaseTableName;
            Result resultado = new Result();
            bool isCached = cache == null ? false : true;
            bool requiresResult = false;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect(obj, out resultado, isCached, tableName, cache, isPartialCache, forceQueryDataBase, useAppConfig);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll(obj, out resultado, isCached, tableName, cache, forceQueryDataBase, useAppConfig);
                    break;
                case TransactionTypes.Delete:
                    requiresResult = true;
                    break;
                case TransactionTypes.Insert:
                    requiresResult = true;
                    break;
                case TransactionTypes.Update:
                    requiresResult = true;
                    break;
                case TransactionTypes.StoredProcedure:
                    resultado = dbOperation.ExecuteProcedure(obj, tableName, transactionType, useAppConfig);
                    break;
                default:
                    break;
            }

            if (requiresResult)
            {
                resultado = dbOperation.ExecuteProcedure(obj, tableName, transactionType, useAppConfig);
                if (transactionType != TransactionTypes.StoredProcedure)
                {
                    if (isCached && resultado.Data.Rows.Count > 0) DeleteInCache(obj, cache);
                }
            }

            return resultado;
        }

        private void EvaluateSelect<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool isPartialCache, bool forceQueryDataBase, bool useAppConfig) where T : new()
        {
            if (forceQueryDataBase)
            {
                resultado = dbOperation.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig);
            }
            else
            {
                resultado = isCached == true ? SelectInCache(obj, cache) : dbOperation.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig);

                resultado.IsFromCache = isCached == true ? true : false;
                if (isCached && isPartialCache && resultado.Data.Rows.Count == 0)
                {
                    resultado = dbOperation.ExecuteProcedure(obj, tableName, TransactionTypes.Select, useAppConfig);
                }
            }
        }

        private void EvaluateSelectAll<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool forceQueryDataBase, bool useAppConfig)
        {
            if (forceQueryDataBase)
            {
                resultado = dbOperation.ExecuteProcedure(obj, tableName, TransactionTypes.SelectAll, useAppConfig);
            }
            else
            {
                resultado = isCached == true ? cache : dbOperation.ExecuteProcedure(obj, tableName, TransactionTypes.SelectAll, useAppConfig);
                resultado.IsFromCache = isCached == true ? true : false;
            }
        }

        private Result SelectInCache<T>(T obj, Result cache) where T : new()
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(UnlinkedProperty)) == null)
                {
                    if (prop.GetValue(obj) != null)
                    {
                        builder.AppendFormat("{0}== @{1} and ", prop.Name, valueIndex);
                        values.Add(prop.GetValue(obj));
                        valueIndex++;
                    }
                }
            }

            predicate = builder.ToString();
            if (string.IsNullOrEmpty(predicate))
            {
                throw new InvalidNumberOfParametersException();
            }
            else
            {
                predicate = predicate.Substring(0, predicate.Length - 5);
                return new Result(data: DataSerializer.ConvertListToDataTableOfType(DataSerializer.ConvertDataTableToListOfType<T>(cache.Data).Where(predicate, values.ToArray()).ToList()));
            }
        }

        private DataRow SetRowData<T>(DataRow row, T obj, bool isInsert)
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

        private void UpdateInCache<T>(T obj, Result cache)
        {
            SetRowData<T>(cache.Data.Rows.Find((obj as Main).Id), obj, false).AcceptChanges();
        }

        private void InsertInCache<T>(T obj, Result cache)
        {
            //(obj as Main).FechaCreacion = DateTime.Now;
            //(obj as Main).FechaModificacion = DateTime.Now;

            cache.Data.Rows.Add(SetRowData(cache.Data.NewRow(), obj, true));
            cache.Data.AcceptChanges();
        }

        public void AlterCache(DataRow row, Result cache)
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

        private void DeleteInCache<T>(T obj, Result cache)
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
