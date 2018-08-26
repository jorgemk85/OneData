using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
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

        public Result Evaluate<T, TKey>(T obj, TransactionTypes transactionType, DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Result resultado = null;
            bool hasCache = dataCache.Cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect<T, TKey>(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll<T, TKey>(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.Delete:
                    EvaluateDelete<T, TKey>(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.Insert:
                    EvaluateInsert<T, TKey>(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.Update:
                    EvaluateUpdate<T, TKey>(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.StoredProcedure:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public Result Evaluate<T, TKey>(List<T> list, TransactionTypes transactionType, ref DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            throw new NotImplementedException();
        }

        private void EvaluateInsert<T, TKey>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Insert);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertInCache<T, TKey>(obj, ref dataCache);
            }
        }

        private void EvaluateUpdate<T, TKey>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Update);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateInCache<T, TKey>(obj, ref dataCache);
            }
        }

        private void EvaluateDelete<T, TKey>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Delete);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteInCache<T, TKey>(obj, ref dataCache);
            }
        }

        private void EvaluateSelect<T, TKey>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = hasCache == true ? SelectInCache<T, TKey>(obj, dataCache) : operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = hasCache == true ? true : false;
                if (hasCache && dataCache.IsPartialCache && resultado.Data.Rows.Count == 0)
                {
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);
                    AlterCache(resultado, ref dataCache);
                }
                if (!resultado.IsFromCache && hasCache)
                {
                    AlterCache(resultado, ref dataCache);
                }
                if (!hasCache && resultado.IsSuccessful)
                {
                    dataCache.Cache = resultado;
                }
            }
        }

        private void EvaluateSelectAll<T, TKey>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.SelectAll);
            }
            else
            {
                if (hasCache && !dataCache.IsPartialCache)
                {
                    resultado = dataCache.Cache;
                    resultado.IsFromCache = true;
                }
                else
                {
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.SelectAll);
                    dataCache.Cache = resultado;
                }
            }

            if (dataCache.IsEnabled && resultado.IsSuccessful && resultado.Data.Rows.Count > 0)
            {
                dataCache.IsPartialCache = false;
            }
        }

        private Result SelectInCache<T, TKey>(T obj, DataCache dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.FilteredProperties)
            {
                if (property.Value.GetValue(obj) != null)
                {
                    builder.AppendFormat("{0} == @{1} and ", property.Value.Name, valueIndex);
                    values.Add(property.Value.GetValue(obj));
                    valueIndex++;
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
                var queryableList = dataCache.Cache.Data.ToList<T>().AsQueryable();
                // Procedimiento LENTO en la primera ejecucion por el compilado del query.
                var resultList = queryableList.Where(predicate, values.ToArray()).ToList().ToDataTable<T, TKey>();
                return new Result(resultList, true, true);
            }
        }

        private DataRow SetRowData<T, TKey>(DataRow row, T obj) where T : Cope<T, TKey>, new() where TKey : struct
        {
            foreach (PropertyInfo property in Manager<T, TKey>.ModelComposition.Properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    row[property.Name] = property.GetValue(obj);
                }
            }
            return row;
        }

        private void UpdateInCache<T, TKey>(T obj, ref DataCache dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            SetRowData<T, TKey>(dataCache.Cache.Data.Rows.Find(obj.Id.GetValueOrDefault()), obj).AcceptChanges();
        }

        private void InsertInCache<T, TKey>(T obj, ref DataCache dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            dataCache.Cache.Data.Rows.Add(SetRowData<T, TKey>(dataCache.Cache.Data.NewRow(), obj));
            dataCache.Cache.Data.AcceptChanges();
        }

        private void InsertListInCache<T, TKey>(List<T> list, DataCache dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Data.Rows.Add(SetRowData<T, TKey>(dataCache.Cache.Data.NewRow(), obj));
            }
            dataCache.Cache.Data.AcceptChanges();
        }

        private void AlterCache(Result resultado, ref DataCache dataCache)
        {
            foreach (DataRow row in resultado.Data.Rows)
            {
                AlterCache(row, ref dataCache);
            }
        }

        public void AlterCache(DataRow row, ref DataCache dataCache)
        {
            DataRow cacheRow = dataCache.Cache.Data.Rows.Find(row[row.Table.PrimaryKey[0]]);
            string columnName = string.Empty;

            if (cacheRow == null)
            {
                // NO existe la fila: la agrega.
                dataCache.Cache.Data.Rows.Add(row.ItemArray);
                dataCache.Cache.Data.AcceptChanges();
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
            dataCache.Cache.Data.AcceptChanges();
        }

        private void DeleteInCache<T, TKey>(T obj, ref DataCache dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            for (int i = 0; i < dataCache.Cache.Data.Rows.Count; i++)
            {
                DataRow row = dataCache.Cache.Data.Rows[i];
                if (row[row.Table.PrimaryKey[0]].Equals(obj.Id.GetValueOrDefault()))
                {
                    row.Delete();
                    dataCache.Cache.Data.AcceptChanges();
                    break;
                }
            }
        }
    }
}
