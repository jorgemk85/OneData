using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
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
        IOperable operation;
        ConnectionTypes connectionType;

        public QueryEvaluation()
        {
            connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
            operation = Operation.GetOperationBasedOnConnectionType(connectionType);
        }

        public Result Evaluate<T>(T obj, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : IManageable, new()
        {
            string tableName = obj.DataBaseTableName;
            Result resultado = null;
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect(obj, out resultado, isCached, tableName, cache, isPartialCache, forceQueryDataBase, connectionToUse);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll(obj, out resultado, isCached, tableName, cache, forceQueryDataBase, connectionToUse);
                    break;
                case TransactionTypes.Delete:
                    resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) DeleteInCache(obj, cache);
                    break;
                case TransactionTypes.Insert:
                    resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) InsertInCache(obj, cache);
                    break;
                case TransactionTypes.Update:
                    resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) UpdateInCache(obj, cache);
                    break;
                case TransactionTypes.StoredProcedure:
                    resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public Result Evaluate<T>(List<T> list, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : IManageable, new()
        {
            string tableName = new T().DataBaseTableName;
            Result resultado = null;
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.InsertList:
                    resultado = operation.ExecuteProcedure(list, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) InsertListInCache(list, cache);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        private void EvaluateSelect<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : IManageable, new()
        {
            if (forceQueryDataBase)
            {
                resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = isCached == true ? SelectInCache(obj, cache) : operation.ExecuteProcedure(obj, tableName, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = isCached == true ? true : false;
                if (isCached && isPartialCache && resultado.Data.Rows.Count == 0)
                {
                    resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, TransactionTypes.Select);
                }
            }
        }

        private void EvaluateSelectAll<T>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool forceQueryDataBase, string connectionToUse) where T : IManageable, new()
        {
            if (forceQueryDataBase)
            {
                resultado = operation.ExecuteProcedure(obj, tableName, connectionToUse, TransactionTypes.SelectAll);
            }
            else
            {
                resultado = isCached == true ? cache : operation.ExecuteProcedure(obj, tableName, connectionToUse, TransactionTypes.SelectAll);
                resultado.IsFromCache = isCached == true ? true : false;
            }
        }

        private Result SelectInCache<T>(T obj, Result cache) where T : IManageable, new()
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (property.GetCustomAttribute<UnlinkedProperty>() == null)
                {
                    if (property.GetValue(obj) != null)
                    {
                        builder.AppendFormat("{0}== @{1} and ", property.Name, valueIndex);
                        values.Add(property.GetValue(obj));
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
                return new Result(DataSerializer.ConvertListToDataTableOfType(DataSerializer.ConvertDataTableToListOfType<T>(cache.Data).Where(predicate, values.ToArray()).ToList()), true, true);
            }
        }

        private DataRow SetRowData<T>(DataRow row, T obj) where T : IManageable
        {
            object value = null;
            Type type;

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    value = property.GetValue(obj);
                    // La base de datos no acepta nulls, entonces verifica si es para asignarle un valor
                    if (value == null)
                    {
                        type = Nullable.GetUnderlyingType(property.PropertyType) != null ? type = Nullable.GetUnderlyingType(property.PropertyType) : type = property.PropertyType;

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
                    row[property.Name] = value;
                }
            }
            return row;
        }

        private void UpdateInCache<T>(T obj, Result cache) where T : IManageable
        {
            SetRowData(cache.Data.Rows.Find(obj.Id), obj).AcceptChanges();
        }

        private void InsertInCache<T>(T obj, Result cache) where T : IManageable
        {
            cache.Data.Rows.Add(SetRowData(cache.Data.NewRow(), obj));
            cache.Data.AcceptChanges();
        }

        private void InsertListInCache<T>(List<T> list, Result cache) where T : IManageable
        {
            foreach (T obj in list)
            {
                cache.Data.Rows.Add(SetRowData(cache.Data.NewRow(), obj));
            }
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

        private void DeleteInCache<T>(T obj, Result cache) where T : IManageable
        {
            for (int i = 0; i < cache.Data.Rows.Count; i++)
            {
                DataRow row = cache.Data.Rows[i];
                if (row[row.Table.PrimaryKey[0]].Equals(obj.Id.GetValueOrDefault()))
                {
                    row.Delete();
                    cache.Data.AcceptChanges();
                    break;
                }
            }
        }
    }
}
