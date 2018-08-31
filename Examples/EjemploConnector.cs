using DataManagement.DAO;
using DataManagement.Extensions;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;

namespace DataManagement.Examples
{
    public class Connector
    {
        const string CONNECTION_TO_USE = "GoDaddy";

        public static T Select<T>(params Parameter[] parameters) where T : Cope<T, Guid>, new()
        {
            return DataSerializer.ConvertManageableCollectionToObjectOfType(Manager<T, Guid>.Select(CONNECTION_TO_USE, parameters).Collection);
        }

        public static Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters) where T : Cope<T, Guid>, new()
        {
            return (Dictionary<Guid, T>)Manager<T, Guid>.Select(CONNECTION_TO_USE, parameters).Collection;
        }

        public static List<T> SelectList<T>(params Parameter[] parameters) where T : Cope<T, Guid>, new()
        {
            return Manager<T, Guid>.Select(CONNECTION_TO_USE, parameters).Collection.ToList();
        }

        //public static string SelectJson<T>(params Parameter[] parameters) where T : Cope<T, Guid>, new()
        //{
        //    return DataSerializer.SerializeDataTableToJsonObjectOfType<T>(Manager<T, Guid>.Select(CONNECTION_TO_USE, parameters).Data);
        //}

        public static Dictionary<Guid, T> SelectAllDictionary<T>() where T : Cope<T, Guid>, new()
        {
            return (Dictionary<Guid, T>)Manager<T, Guid>.SelectAll(CONNECTION_TO_USE).Collection;
        }

        public static List<T> SelectAllList<T>() where T : Cope<T, Guid>, new()
        {
            return Manager<T, Guid>.SelectAll(CONNECTION_TO_USE).Collection.ToList();
        }

        //public static string SelectAllJson<T>() where T : Cope<T, Guid>, new()
        //{
        //    return DataSerializer.SerializeDataTableToJsonListOfType<T>(Manager<T, Guid>.SelectAll(CONNECTION_TO_USE).Data);
        //}
    }
}
