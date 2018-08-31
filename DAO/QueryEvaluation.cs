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

        public Result<T, TKey> Evaluate<T, TKey>(T obj, TransactionTypes transactionType, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Result<T, TKey> resultado = null;
            bool hasCache = dataCache.Cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.Delete:
                    EvaluateDelete(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.Insert:
                    EvaluateInsert(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.Update:
                    EvaluateUpdate(obj, out resultado, hasCache, ref dataCache, connectionToUse);
                    break;
                case TransactionTypes.StoredProcedure:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public Result Evaluate<T, TKey>(IEnumerable<T> list, TransactionTypes transactionType, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            throw new NotImplementedException();
        }

        private void EvaluateInsert<T, TKey>(T obj, out Result<T, TKey> resultado, bool hasCache, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Insert);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertInCache(obj, ref dataCache);
            }
        }

        private void EvaluateUpdate<T, TKey>(T obj, out Result<T, TKey> resultado, bool hasCache, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Update);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateInCache(obj, ref dataCache);
            }
        }

        private void EvaluateDelete<T, TKey>(T obj, out Result<T, TKey> resultado, bool hasCache, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Delete);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteInCache(obj, ref dataCache);
            }
        }

        private void EvaluateSelect<T, TKey>(T obj, out Result<T, TKey> resultado, bool hasCache, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = hasCache == true ? SelectInCache(obj, dataCache) : operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = hasCache == true ? true : false;
                if (hasCache && dataCache.IsPartialCache && resultado.Collection.Count == 0)
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
                    dataCache.LastCacheUpdate = DateTime.Now.Ticks;
                }
            }
        }

        private void EvaluateSelectAll<T, TKey>(T obj, out Result<T, TKey> resultado, bool hasCache, ref DataCache<T, TKey> dataCache, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
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
                    dataCache.LastCacheUpdate = DateTime.Now.Ticks;
                }
            }

            if (dataCache.IsEnabled && resultado.IsSuccessful && resultado.Collection.Count > 0)
            {
                dataCache.IsPartialCache = false;
            }
        }

        private Result<T, TKey> SelectInCache<T, TKey>(T obj, DataCache<T, TKey> dataCache) where T : Cope<T, TKey>, new() where TKey : struct
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
                var queryableList = dataCache.Cache.Collection.AsQueryable();
                // Procedimiento LENTO en la primera ejecucion por el compilado del query.
                var resultList = queryableList.Where(predicate, values.ToArray()).ToDictionary(k => k.Key, v => v.Value);
                return new Result<T, TKey>((ManageableCollection<TKey, T>)resultList, true, true);
            }
        }

        private void UpdateInCache<T, TKey>(T obj, ref DataCache<T, TKey> dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            dataCache.Cache.Collection[obj.Id.GetValueOrDefault()] = obj;
        }

        private void InsertInCache<T, TKey>(T obj, ref DataCache<T, TKey> dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            dataCache.Cache.Collection.Add(obj.Id.GetValueOrDefault(), obj);
        }

        private void InsertMassiveInCache<T, TKey>(IEnumerable<T> list, ref DataCache<T, TKey> dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Collection.Add(obj.Id.GetValueOrDefault(), obj);
            }
        }

        private void AlterCache<T, TKey>(Result<T, TKey> resultado, ref DataCache<T, TKey> dataCache)
        {
            foreach (KeyValuePair<TKey, T> row in resultado.Collection)
            {
                AlterCache(row, ref dataCache);
            }
            dataCache.LastCacheUpdate = DateTime.Now.Ticks;
        }

        public void AlterCache<T, TKey>(KeyValuePair<TKey, T> row, ref DataCache<T, TKey> dataCache)
        {
            dataCache.Cache.Collection.TryGetValue(row.Key, out T cachedObj);

            if (cachedObj == null)
            {
                // NO existe la fila: la agrega.
                dataCache.Cache.Collection.Add(row.Key, row.Value);
            }
            else
            {
                // SI existe la fila: la actualiza.
                dataCache.Cache.Collection[row.Key] = row.Value;
            }
        }

        private void DeleteInCache<T, TKey>(T obj, ref DataCache<T, TKey> dataCache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            dataCache.Cache.Collection.Remove(obj.Id.GetValueOrDefault());
        }
    }
}
