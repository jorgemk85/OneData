using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Extensions
{
    public static class DataTableExtensions
    {
        public static T ToObject<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.ConvertDataTableToObjectOfType<T>(dataTable);
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this DataTable dataTable, string keyName, string valueName)
        {
            return DataSerializer.ConvertDataTableToDictionary<TKey, TValue>(dataTable, keyName, valueName);
        }

        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this DataTable dataTable, string keyName) where T : new()
        {
            return DataSerializer.ConvertDataTableToDictionary<TKey, T>(dataTable, keyName);
        }

        public static ManageableCollection<TKey, T> ToManageableCollection<TKey, T>(this DataTable dataTable) where T : Cope<T, TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertDataTableToManageableCollectionOfType<TKey, T>(dataTable);
        }

        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this DataTable dataTable) where T : Cope<T, TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<TKey, T>(dataTable);
        }

        public static List<T> ToList<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(dataTable);
        }

        public static ICollection<dynamic> ToList(this DataTable dataTable, Type target)
        {
            return DataSerializer.ConvertDataTableToListOfType(dataTable, target);
        }

        public static HashSet<T> ToHashSet<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.ConvertDataTableToHashSetOfType<T>(dataTable);
        }

        public static string ToJson<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.SerializeDataTableToJsonListOfType<T>(dataTable);
        }

        public static string ToXml<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.SerializeDataTableToXmlListOfType<T>(dataTable);
        }
    }
}

