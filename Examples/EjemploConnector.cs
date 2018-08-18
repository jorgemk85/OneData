using DataManagement.Models;
using DataManagement.DAO;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using DataManagement.Interfaces;

namespace DataManagement.Examples
{
    public class Connector
    {
        const string CONNECTION_TO_USE = "GoDaddy";

        public static Result ExecuteStoredProcedure(string tableName, string storedProcedure, params Parameter[] parameters)
        {
            return Manager.StoredProcedure(tableName, storedProcedure, CONNECTION_TO_USE, parameters);
        }

        public static T Select<T>(params Parameter[] parameters) where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToObjectOfType<T>(Manager<T>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        public static Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters) where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<T>(Manager<T>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        public static List<T> SelectList<T>(params Parameter[] parameters) where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(Manager<T>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        public static string SelectJson<T>(params Parameter[] parameters) where T : IManageable, new()
        {
            return DataSerializer.SerializeDataTableToJsonObjectOfType<T>(Manager<T>.Select(CONNECTION_TO_USE, parameters).Data);
        }

        public static Dictionary<Guid, T> SelectAllDictionary<T>() where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<T>(Manager<T>.SelectAll(CONNECTION_TO_USE).Data);
        }

        public static List<T> SelectAllList<T>() where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(Manager<T>.SelectAll(CONNECTION_TO_USE).Data);
        }

        public static string SelectAllJson<T>() where T : IManageable, new()
        {
            return DataSerializer.SerializeDataTableToJsonListOfType<T>(Manager<T>.SelectAll(CONNECTION_TO_USE).Data);
        }
    }
}
