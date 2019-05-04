using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace OneData.Extensions
{
    public static class IEnumerableExtensions
    {
        //public static void ToExcel<T>(this IEnumerable<T> list, string fullyQualifiedFileName)
        //{
        //    ExcelSerializer.SerializeIEnumerableOfTypeToExcel(list, fullyQualifiedFileName);
        //}

        //public static void FromExcel<T>(this IEnumerable<T> list, ref List<T> myList, string worksheetName, string fullyQualifiedFileName) where T : new()
        //{
        //    myList = ExcelSerializer.DeserializeExcelToListOfType<T>(worksheetName, fullyQualifiedFileName);
        //}

        public static void ToFile<T>(this IEnumerable<T> list, string fullyQualifiedFileName, char separator)
        {
            FileSerializer.SerializeIEnumerableOfTypeToFile(list, fullyQualifiedFileName, separator);
        }

        public static void FromFile<T>(this IEnumerable<T> list, ref List<T> myList, string fullyQualifiedFileName, char separator, Encoding encoding) where T : new()
        {
            myList = FileSerializer.DeserializeFileToListOfType<T>(fullyQualifiedFileName, separator, encoding);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : new()
        {
            return DataSerializer.ConvertIEnumerableToDataTableOfGenericType(list);
        }

        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this IEnumerable<T> list, string keyPropertyName)
        {
            return DataSerializer.ConvertIEnumerableToDictionaryOfType<TKey, T>(list, keyPropertyName);
        }

        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this IEnumerable<T> list, Expression<Func<T, TKey>> keyProperty)
        {
            return DataSerializer.ConvertIEnumerableToDictionaryOfType(list, keyProperty);
        }

        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this IEnumerable<T> list) where T : Cope<T>, IManageable, new()
        {
            return DataSerializer.ConvertIEnumerableToDictionaryOfType<TKey, T>(list);
        }
    }
}
