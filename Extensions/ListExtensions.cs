using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using DataManagement.Standard.Tools;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataManagement.Standard.Extensions
{
    public static class ListExtensions
    {
        public static void ToExcel<T>(this List<T> list, string fullyQualifiedFileName)
        {
            ExcelSerializer.SerializeListOfTypeToExcel(list, fullyQualifiedFileName);
        }

        public static void FromExcel<T>(this List<T> list, ref List<T> myList, string worksheetName, string fullyQualifiedFileName) where T : new()
        {
            myList = ExcelSerializer.DeserializeExcelToListOfType<T>(worksheetName, fullyQualifiedFileName);
        }

        public static void ToFile<T>(this List<T> list, string fullyQualifiedFileName, char separator)
        {
            FileSerializer.SerializeListOfTypeToFile(list, fullyQualifiedFileName, separator);
        }

        public static void FromFile<T>(this List<T> list, ref List<T> myList, string fullyQualifiedFileName, char separator, Encoding encoding) where T : new()
        {
            myList = FileSerializer.DeserializeFileToListOfType<T>(fullyQualifiedFileName, separator, encoding);
        }

        public static DataTable ToDataTable<T>(this List<T> list) where T : new()
        {
            return DataSerializer.ConvertListToDataTableOfGenericType(list);
        }

        public static DataTable ToDataTable<T, TKey>(this List<T> list) where T : Cope<T, TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertListToDataTableOfType<T, TKey>(list);
        }
    }
}
