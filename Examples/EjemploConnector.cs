using DataManagement.Standard.DAO;
using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using DataManagement.Standard.Tools;
using System.Collections.Generic;

namespace DataManagement.Standard.Examples
{
    public class Connector
    {
        const string CONNECTION_TO_USE = "GoDaddy";

        public static Result ExecuteStoredProcedure(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return Manager.StoredProcedure(tableName, storedProcedure, CONNECTION_TO_USE, parameters);
        }

        public static T Select<T, TKey>(params Parameter[] parameters) where T : IManageable<TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertDataTableToObjectOfType<T>(Manager<T, TKey>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        //public static Dictionary<Guid, T> SelectDictionary<T, TKey>(params Parameter[] parameters) where T : IManageable<TKey>, new()
        //{
        //    return DataSerializer.ConvertDataTableToDictionaryOfType<T>(Manager<T, TKey>.Select(CONNECTION_TO_USE, parameters).Data);
        //}

        public static List<T> SelectList<T, TKey>(params Parameter[] parameters) where T : IManageable<TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(Manager<T, TKey>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        public static string SelectJson<T, TKey>(params Parameter[] parameters) where T : IManageable<TKey>, new() where TKey : struct
        {
            return DataSerializer.SerializeDataTableToJsonObjectOfType<T>(Manager<T, TKey>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        //public static Dictionary<Guid, T> SelectAllDictionary<T, TKey>() where T : IManageable<TKey>, new()
        //{
        //    return DataSerializer.ConvertDataTableToDictionaryOfType<T>(Manager<T, TKey>.SelectAll(CONNECTION_TO_USE).Data);
        //}

        public static List<T> SelectAllList<T, TKey>() where T : IManageable<TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(Manager<T, TKey>.SelectAll(CONNECTION_TO_USE).Data);
        }

        public static string SelectAllJson<T, TKey>() where T : IManageable<TKey>, new() where TKey : struct
        {
            return DataSerializer.SerializeDataTableToJsonListOfType<T>(Manager<T, TKey>.SelectAll(CONNECTION_TO_USE).Data);
        }
    }
}
