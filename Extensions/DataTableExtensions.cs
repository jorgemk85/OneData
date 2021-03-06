﻿using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace OneData.Extensions
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

        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this DataTable dataTable) where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<TKey, T>(dataTable);
        }

        public static Dictionary<dynamic, T> ToDictionary<T>(this DataTable dataTable, string keyName, Type keyType) where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToDictionaryOfType<T>(dataTable, keyName, keyType);
        }

        public static List<T> ToList<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(dataTable);
        }

        public static ICollection<dynamic> ToList(this DataTable dataTable, Type target)
        {
            return DataSerializer.ConvertDataTableToListOfType(dataTable, target);
        }

        public static Hashtable ToHashtable(this DataTable dataTable, string keyName)
        {
            return DataSerializer.ConvertDataTableToHashtable(dataTable, keyName);
        }

        public static Hashtable ToHashtable<T>(this DataTable dataTable) where T : IManageable, new()
        {
            return DataSerializer.ConvertDataTableToHashtableOfType<T>(dataTable);
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

