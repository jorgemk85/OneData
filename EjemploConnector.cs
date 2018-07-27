using DataAccess.BO;
using DataAccess.DAO;
using System;
using System.Collections.Generic;

namespace DataAccess
{
    public class Connector
    {
        const bool USE_APP_CONFIG = true;

        public static Result Delete<T>(T obj) where T : new()
        {
            return DataManagement<T>.Delete(obj, USE_APP_CONFIG);
        }

        public static Result Insert<T>(T obj) where T : new()
        {
            return DataManagement<T>.Insert(obj, USE_APP_CONFIG);
        }

        public static Result Update<T>(T obj) where T : new()
        {
            return DataManagement<T>.Update(obj, USE_APP_CONFIG);
        }

        public static Result ExecuteStoredProcedure(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return DataManagement<Object>.Select(tableName, storedProcedure, USE_APP_CONFIG, parameters);
        }

        public static T Select<T>(params Parameter[] parameters) where T : new()
        {
            return Tools.ConvertDataTableToObjectOfType<T>(DataManagement<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters) where T : new()
        {
            return Tools.ConvertDataTableToDictionaryOfType<T>(DataManagement<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static List<T> SelectList<T>(params Parameter[] parameters) where T : new()
        {
            return Tools.ConvertDataTableToListOfType<T>(DataManagement<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static string SelectJson<T>(params Parameter[] parameters) where T : new()
        {
            return Tools.ConvertDataTableToJsonObjectOfType<T>(DataManagement<T>.Select(USE_APP_CONFIG, parameters).Data);
        }

        public static Dictionary<Guid, T> SelectAllDictionary<T>() where T : new()
        {
            return Tools.ConvertDataTableToDictionaryOfType<T>(DataManagement<T>.SelectAll(USE_APP_CONFIG).Data);
        }

        public static List<T> SelectAllList<T>() where T : new()
        {
            return Tools.ConvertDataTableToListOfType<T>(DataManagement<T>.SelectAll(USE_APP_CONFIG).Data);
        }

        public static string SelectAllJson<T>() where T : new()
        {
            return Tools.ConvertDataTableToJsonListOfType<T>(DataManagement<T>.SelectAll(USE_APP_CONFIG).Data);
        }
    }
}
