using DataManagement.Tools;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataManagement.Extensions
{
    public static class ListExtensions
    {
        public static void ToExcel<T>(this List<T> list, string fullyQualifiedFileName)
        {
            ExcelSerializer.SerializeListOfTypeToExcel(list, fullyQualifiedFileName);
        }

        public static List<T> FromExcel<T>(this List<T> list, string worksheetName, string fullyQualifiedFileName) where T : new()
        {
            return ExcelSerializer.DeserializeExcelToListOfType<T>(worksheetName, fullyQualifiedFileName);
        }

        public static void ToFile<T>(this List<T> list, string fullyQualifiedFileName, char separator)
        {
            FileSerializer.SerializeListOfTypeToFile(list, fullyQualifiedFileName, separator);
        }

        public static List<T> FromFile<T>(this List<T> list, string fullyQualifiedFileName, char separator, Encoding encoding) where T : new()
        {
            return FileSerializer.DeserializeFileToListOfType<T>(fullyQualifiedFileName, separator, encoding);
        }

        public static DataTable ToDataTable<T>(this List<T> list) where T : new()
        {
            return DataSerializer.ConvertListToDataTableOfType<T>(list);
        }
    }
}
