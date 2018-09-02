using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Collections;
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

        public Result Evaluate<T>(T obj, TransactionTypes transactionType, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            Result resultado = null;
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

        public Result Evaluate<T>(IEnumerable<T> list, TransactionTypes transactionType, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            throw new NotImplementedException();
        }

        private void EvaluateInsert<T>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Insert);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertInCache(obj, ref dataCache);
            }
        }

        private void EvaluateUpdate<T>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Update);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateInCache(obj, ref dataCache);
            }
        }

        private void EvaluateDelete<T>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Delete);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteInCache(obj, ref dataCache);
            }
        }

        private void EvaluateSelect<T>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure(obj, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = hasCache == true ? SelectInCache(obj, dataCache) : operation.ExecuteProcedure<T>(obj, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = hasCache == true ? true : false;
                if (hasCache && dataCache.IsPartialCache && resultado.Hash.Count == 0)
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

        private void EvaluateSelectAll<T>(T obj, out Result resultado, bool hasCache, ref DataCache dataCache, string connectionToUse) where T : Cope<T>, IManageable, new()
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

            if (dataCache.IsEnabled && resultado.IsSuccessful && resultado.Hash.Count > 0)
            {
                dataCache.IsPartialCache = false;
            }
        }

        private Result SelectInCache<T>(T obj, DataCache dataCache) where T : Cope<T>, IManageable, new()
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T>.ModelComposition.FilteredProperties)
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
                var queryableList = dataCache.Cache.Hash.AsQueryable();
                // Procedimiento LENTO en la primera ejecucion por el compilado del query.
                var resultList = queryableList.Where(predicate, values.ToArray());
                return new Result((Hashtable)resultList, true, true);
            }
        }

        private void UpdateInCache<T>(T obj, ref DataCache dataCache) where T : Cope<T>, IManageable, new()
        {
            dataCache.Cache.Hash[obj.ModelComposition.PrimaryProperty.GetValue(obj)] = obj;
        }

        private void InsertInCache<T>(T obj, ref DataCache dataCache) where T : Cope<T>, IManageable, new()
        {
            dataCache.Cache.Hash.Add(obj.ModelComposition.PrimaryProperty.GetValue(obj), obj);
        }

        private void InsertMassiveInCache<T>(IEnumerable<T> list, ref DataCache dataCache) where T : Cope<T>, IManageable, new()
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Hash.Add(obj.ModelComposition.PrimaryProperty.GetValue(obj), obj);
            }
        }

        private void AlterCache(Result resultado, ref DataCache dataCache)
        {
            foreach (DictionaryEntry row in resultado.Hash)
            {
                AlterCache(row, ref dataCache);
            }
            dataCache.LastCacheUpdate = DateTime.Now.Ticks;
        }

        public void AlterCache(DictionaryEntry row, ref DataCache dataCache)
        {
            if (dataCache.Cache.Hash.ContainsKey(row.Key))
            {
                // SI existe la fila: la actualiza.
                dataCache.Cache.Hash[row.Key] = row.Value;
            }
            else
            {
                // NO existe la fila: la agrega.
                dataCache.Cache.Hash.Add(row.Key, row.Value);
            }
        }

        private void DeleteInCache<T>(T obj, ref DataCache dataCache) where T : Cope<T>, IManageable, new()
        {
            dataCache.Cache.Hash.Remove(obj.ModelComposition.PrimaryProperty.GetValue(obj));
        }
    }
}
