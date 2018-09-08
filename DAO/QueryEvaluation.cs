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

        public Result<T> Evaluate<T>(T obj, TransactionTypes transactionType, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            Result<T> resultado = null;
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
                    resultado = operation.ExecuteProcedure(obj, connectionToUse, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public Result<T> Evaluate<T>(IEnumerable<T> list, TransactionTypes transactionType, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            throw new NotImplementedException();
        }

        private void EvaluateInsert<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Insert);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertInCache(obj, ref dataCache);
            }
        }

        private void EvaluateUpdate<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Update);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateInCache(obj, ref dataCache);
            }
        }

        private void EvaluateDelete<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Delete);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteInCache(obj, ref dataCache);
            }
        }

        private void EvaluateSelect<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = hasCache == true ? SelectInCache(obj, dataCache) : operation.ExecuteProcedure<T>(obj, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = hasCache == true ? true : false;
                if (hasCache && dataCache.IsPartialCache && resultado.Data.Count == 0)
                {
                    resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Select);
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

        private void EvaluateSelectAll<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.SelectAll);
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
                    resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.SelectAll);
                    dataCache.Cache = resultado;
                    dataCache.LastCacheUpdate = DateTime.Now.Ticks;
                }
            }

            if (dataCache.IsEnabled && resultado.IsSuccessful && resultado.Data.Count > 0)
            {
                dataCache.IsPartialCache = false;
            }
        }

        private Result<T> SelectInCache<T>(T obj, DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.FilteredProperties)
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
                var queryableList = dataCache.Cache.Data.Values.AsQueryable();
                // Procedimiento LENTO en la primera ejecucion por el compilado del query.
                var resultList = queryableList.Where(predicate, values.ToArray()).ToDictionary(Cope<T>.ModelComposition.PrimaryKeyProperty.Name, Cope<T>.ModelComposition.PrimaryKeyProperty.PropertyType);
                return new Result<T>(resultList, true, true);
            }
        }

        private void UpdateInCache<T>(T obj, ref DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            dataCache.Cache.Data[Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)] = obj;
        }

        private void InsertInCache<T>(T obj, ref DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            dataCache.Cache.Data.Add(Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj), obj);
        }

        private void InsertMassiveInCache<T>(IEnumerable<T> list, ref DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Data.Add(Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj), obj);
            }
        }

        private void AlterCache<T>(Result<T> resultado, ref DataCache<T> dataCache)
        {
            foreach (KeyValuePair<dynamic, T> item in resultado.Data)
            {
                AlterCache(item, ref dataCache);
            }
            dataCache.LastCacheUpdate = DateTime.Now.Ticks;
        }

        public void AlterCache<T>(KeyValuePair<dynamic, T> item, ref DataCache<T> dataCache)
        {
            if (dataCache.Cache.Data.ContainsKey(item.Key))
            {
                // SI existe la fila: la actualiza.
                dataCache.Cache.Data[item.Key] = item.Value;
            }
            else
            {
                // NO existe la fila: la agrega.
                dataCache.Cache.Data.Add(item.Key, item.Value);
            }
        }

        private void DeleteInCache<T>(T obj, ref DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            dataCache.Cache.Data.Remove(obj);
        }
    }
}
