using DataManagement.Tools;
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

        public static List<T> ToList<T>(this DataTable dataTable) where T : new()
        {
            return DataSerializer.ConvertDataTableToListOfType<T>(dataTable);
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

