using DataManagement.Standard.Attributes;
using DataManagement.Standard.Enums;
using DataManagement.Standard.Exceptions;
using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using DataManagement.Standard.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataManagement.Standard.DAO
{
    internal class QueryEvaluation
    {
        IOperable operation;
        ConnectionTypes connectionType;

        public QueryEvaluation(ConnectionTypes connectionType)
        {
            this.connectionType = connectionType;
            operation = Operation.GetOperationBasedOnConnectionType(connectionType);
        }

        public Result Evaluate<T, TKey>(T obj, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : IManageable<TKey>, new()
        {
            string tableName = obj.DataBaseTableName;
            Result resultado = null;
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect<T, TKey>(obj, out resultado, isCached, tableName, cache, isPartialCache, forceQueryDataBase, connectionToUse);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll<T, TKey>(obj, out resultado, isCached, tableName, cache, forceQueryDataBase, connectionToUse);
                    break;
                case TransactionTypes.Delete:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) DeleteInCache<T, TKey>(obj, cache);
                    break;
                case TransactionTypes.Insert:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) InsertInCache<T, TKey>(obj, cache);
                    break;
                case TransactionTypes.Update:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) UpdateInCache<T, TKey>(obj, cache);
                    break;
                case TransactionTypes.StoredProcedure:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public Result Evaluate<T, TKey>(List<T> list, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : IManageable<TKey>, new()
        {
            string tableName = new T().DataBaseTableName;
            Result resultado = null;
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.InsertList:
                    resultado = operation.ExecuteProcedure<T, TKey>(list, tableName, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) InsertListInCache<T, TKey>(list, cache);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        private void EvaluateSelect<T, TKey>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : IManageable<TKey>, new()
        {
            if (forceQueryDataBase)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = isCached == true ? SelectInCache<T, TKey>(obj, cache) : operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = isCached == true ? true : false;
                if (isCached && isPartialCache && resultado.Data.Rows.Count == 0)
                {
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, TransactionTypes.Select);
                }
            }
        }

        private void EvaluateSelectAll<T, TKey>(T obj, out Result resultado, bool isCached, string tableName, Result cache, bool forceQueryDataBase, string connectionToUse) where T : IManageable<TKey>, new()
        {
            if (forceQueryDataBase)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, TransactionTypes.SelectAll);
            }
            else
            {
                resultado = isCached == true ? cache : operation.ExecuteProcedure<T, TKey>(obj, tableName, connectionToUse, TransactionTypes.SelectAll);
                resultado.IsFromCache = isCached == true ? true : false;
            }
        }

        private Result SelectInCache<T, TKey>(T obj, Result cache) where T : IManageable<TKey>, new()
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
                // TODO: Hay que migrar de la version vieja de Linq.Dynamic a la nueva que trabaja con .net core y standard.
                return new Result(cache.Data, true, true);
                //return new Result(DataSerializer.ConvertListToDataTableOfType(DataSerializer.ConvertDataTableToListOfType<T, TKey>(cache.Data)
                //                                                             .Where(predicate,""), true, true);
            }
        }

        private DataRow SetRowData<T, TKey>(DataRow row, T obj) where T : IManageable<TKey>
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

        private void UpdateInCache<T, TKey>(T obj, Result cache) where T : IManageable<TKey>
        {
            SetRowData<T, TKey>(cache.Data.Rows.Find(obj.Id), obj).AcceptChanges();
        }

        private void InsertInCache<T, TKey>(T obj, Result cache) where T : IManageable<TKey>
        {
            cache.Data.Rows.Add(SetRowData<T, TKey>(cache.Data.NewRow(), obj));
            cache.Data.AcceptChanges();
        }

        private void InsertListInCache<T, TKey>(List<T> list, Result cache) where T : IManageable<TKey>
        {
            foreach (T obj in list)
            {
                cache.Data.Rows.Add(SetRowData<T, TKey>(cache.Data.NewRow(), obj));
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

        private void DeleteInCache<T, TKey>(T obj, Result cache) where T : IManageable<TKey>
        {
            for (int i = 0; i < cache.Data.Rows.Count; i++)
            {
                DataRow row = cache.Data.Rows[i];
                if (row[row.Table.PrimaryKey[0]].Equals(obj.Id))
                {
                    row.Delete();
                    cache.Data.AcceptChanges();
                    break;
                }
            }
        }
    }
}
