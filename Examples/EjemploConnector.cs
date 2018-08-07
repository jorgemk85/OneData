using DataManagement.Models;
using DataManagement.DAO;
using DataManagement.Tools;
using System;
using System.Collections.Generic;

namespace DataManagement.Examples
{
    public class Connector
    {
        const bool USE_APP_CONFIG = true;

        public static Result Delete<T>(T obj) where T : new()
        {
            return Manager<T>.Delete(obj, USE_APP_CONFIG);
        }

        public static Result Insert<T>(T obj) where T : new()
        {
            return Manager<T>.Insert(obj, USE_APP_CONFIG);
        }

        public static Result Update<T>(T obj) where T : new()
        {
            return Manager<T>.Update(obj, USE_APP_CONFIG);
        }

        public static Result ExecuteStoredProcedure(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return Manager<Object>.Select(tableName, storedProcedure, USE_APP_CONFIG, parameters);
        }

        public static T Select<T>(params Parameter[] parameters) where T : new()
        {
            return DataSerializer.ConvertDataTableToObjectOfType<T>(Manager<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters) where T : new()
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<T>(Manager<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static List<T> SelectList<T>(params Parameter[] parameters) where T : new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(Manager<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static string SelectJson<T>(params Parameter[] parameters) where T : new()
        {
            return DataSerializer.SerializeDataTableToJsonObjectOfType<T>(Manager<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static Dictionary<Guid, T> SelectAllDictionary<T>() where T : new()
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<T>(Manager<T>.SelectAll(USE_APP_CONFIG).Data);
        }

        public static List<T> SelectAllList<T>() where T : new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(Manager<T>.SelectAll(USE_APP_CONFIG).Data);
        }

        public static string SelectAllJson<T>() where T : new()
        {
            return DataSerializer.SerializeDataTableToJsonListOfType<T>(Manager<T>.SelectAll(USE_APP_CONFIG).Data);
        }
    }
}
