using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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

        public Result<T> Evaluate<T>(T obj, Expression<Func<T, bool>> expression, TransactionTypes transactionType, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            Result<T> resultado = null;
            bool hasCache = dataCache.Cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelectQuery(expression, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll(obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.Delete:
                    EvaluateDelete(obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.Insert:
                    EvaluateInsert(obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.Update:
                    EvaluateUpdate(obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
            }

            return resultado;
        }

        private void EvaluateSelectQuery<T>(Expression<Func<T, bool>> expression, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure(queryOptions, TransactionTypes.Select, true, null, expression);
            }
            else
            {
                resultado = hasCache == true ? SelectInCache(expression, dataCache) : operation.ExecuteProcedure(queryOptions, TransactionTypes.Select, true, null, expression);

                if (hasCache && dataCache.IsPartialCache && resultado.Data.Count == 0)
                {
                    resultado = operation.ExecuteProcedure(queryOptions, TransactionTypes.Select, true, null, expression);
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

                if (resultado.IsFromCache)
                {
                    resultado.Data = GetDataBasedFromCacheOnQueryOptions(ref dataCache, queryOptions);
                }
            }
        }

        private void EvaluateInsert<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.Insert, true, obj, null);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertInCache(obj, ref dataCache);
            }
        }

        private void EvaluateUpdate<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.Update, true, obj, null);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateInCache(obj, ref dataCache);
            }
        }

        private void EvaluateDelete<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.Delete, true, obj, null);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteInCache(obj, ref dataCache);
            }
        }

        private void EvaluateSelectAll<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.SelectAll, true, obj, null);
            }
            else
            {
                if (hasCache && !dataCache.IsPartialCache)
                {
                    resultado = new Result<T>(GetDataBasedFromCacheOnQueryOptions(ref dataCache, queryOptions), true, true);
                }
                else
                {
                    resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.SelectAll, true, obj, null);
                    dataCache.Cache = resultado;
                }
            }

            if (dataCache.IsEnabled && resultado.IsSuccessful && resultado.Data.Count > 0 && queryOptions.MaximumResults == -1 & queryOptions.Offset == 0)
            {
                dataCache.IsPartialCache = false;
            }
        }

        private Dictionary<dynamic, T> GetDataBasedFromCacheOnQueryOptions<T>(ref DataCache<T> dataCache, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            if (queryOptions.Offset > 0 && queryOptions.MaximumResults > 0)
            {
                return new Dictionary<dynamic, T>(dataCache.Cache.Data.Values.OrderByDescending(obj => Cope<T>.ModelComposition.DateModifiedProperty.GetValue(obj)).Skip(queryOptions.Offset).Take(queryOptions.MaximumResults).ToDictionary<dynamic, T>());
            }
            else if (queryOptions.Offset > 0)
            {
                return new Dictionary<dynamic, T>(dataCache.Cache.Data.Values.OrderByDescending(obj => Cope<T>.ModelComposition.DateModifiedProperty.GetValue(obj)).Skip(queryOptions.Offset).ToDictionary<dynamic, T>());
            }
            else if (queryOptions.MaximumResults > 0)
            {
                return new Dictionary<dynamic, T>(dataCache.Cache.Data.Values.OrderByDescending(obj => Cope<T>.ModelComposition.DateModifiedProperty.GetValue(obj)).Take(queryOptions.MaximumResults).ToDictionary<dynamic, T>());
            }

            return dataCache.Cache.Data;
        }

        private Result<T> SelectInCache<T>(Expression<Func<T, bool>> expression, DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            IQueryable<T> queryableList = dataCache.Cache.Data.Values.AsQueryable();
            Dictionary<dynamic, T> resultList = queryableList.Where(expression).ToDictionary(Cope<T>.ModelComposition.PrimaryKeyProperty.Name, Cope<T>.ModelComposition.PrimaryKeyProperty.PropertyType);

            return new Result<T>(resultList, true, true);
        }

        private void UpdateInCache<T>(T obj, ref DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            // TODO: En vez de especificar cada propiedad automatica, es mejor barrer la coleccion de propiedades automaticas y asignar el valor que corresponda.
            Cope<T>.ModelComposition.DateModifiedProperty.SetValue(obj, DateTime.Now);
            dataCache.Cache.Data[Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)] = obj;
        }

        private void InsertInCache<T>(T obj, ref DataCache<T> dataCache) where T : Cope<T>, IManageable, new()
        {
            // TODO: En vez de especificar cada propiedad automatica, es mejor barrer la coleccion de propiedades automaticas y asignar el valor que corresponda.
            Cope<T>.ModelComposition.DateCreatedProperty.SetValue(obj, DateTime.Now);
            Cope<T>.ModelComposition.DateModifiedProperty.SetValue(obj, DateTime.Now);
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
