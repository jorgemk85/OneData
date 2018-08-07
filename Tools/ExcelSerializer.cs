using DataManagement.Attributes;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DataManagement.Tools
{
    public class ExcelSerializer
    {
        /// <summary>
        /// Serializa un objeto de tipo List<typeparamref name="T"/> y lo guarda en u archivo Excel.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto.</typeparam>
        /// <param name="list">Lista de objetos de tipo <typeparamref name="T"/> que se convertira.</param>
        /// <param name="fullyQualifiedFileName">Directorio completo, incluyendo nombre de archivo y extension. Se utiliza para guardar el producto final.</param>
        public static void SerializeListOfTypeToExcel<T>(List<T> list, string fullyQualifiedFileName)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Pagina 1");

                SetExcelHeadersAndTypes(excelWorksheet, ref properties);
                SetExcelContent<T>(excelWorksheet, list, ref properties);
                excelWorksheet.Cells[excelWorksheet.Dimension.Address].AutoFitColumns();
                excelPackage.SaveAs(new FileInfo(fullyQualifiedFileName));
            }
        }

        /// <summary>
        /// Deserializa un archivo de Excel y lo convierte en un objeto List de tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto.</typeparam>
        /// <param name="worksheetName">Nombre de la pagina donde se encuentra la informacion a deserializar.</param>
        /// <param name="fullyQualifiedFileName">Direccion completa del archivo de Excel. Incluir directorio, nombre y extension.</param>
        /// <returns></returns>
        public static List<T> DeserializeExcelToListOfType<T>(string worksheetName, string fullyQualifiedFileName) where T : new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(fullyQualifiedFileName)))
            {
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[worksheetName];

                try
                {
                    return GetExcelContent<T>(excelWorksheet, GetHeadersFromString(excelWorksheet), ref properties);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private static List<T> GetExcelContent<T>(ExcelWorksheet excelWorksheet, List<string> headers, ref PropertyInfo[] properties) where T : new()
        {
            string propertyName = string.Empty;
            HeaderName headerName;
            ExcelAddressBase dimension = excelWorksheet.Dimension;
            List<T> newList = new List<T>();

            for (int y = 0; y < dimension.End.Row - 1; y++)
            {
                T newObj = new T();
                foreach (string header in headers)
                {
                    for (int x = 0; x < properties.Length; x++)
                    {
                        propertyName = properties[x].Name;
                        headerName = properties[x].GetCustomAttribute<HeaderName>();
                        if (headerName != null)
                        {
                            propertyName = headerName.Name;
                        }

                        if (header.Equals(propertyName))
                        {
                            properties[x].SetValue(newObj, SimpleConverter.ConvertStringToType(excelWorksheet.Cells[y + 2, x + 1].Value.ToString(), properties[x].PropertyType));
                            break;
                        }
                    }
                }
                newList.Add(newObj);
            }

            return newList;
        }

        private static List<string> GetHeadersFromString(ExcelWorksheet excelWorksheet)
        {
            List<string> headers = new List<string>();

            ExcelCellAddress end = excelWorksheet.Dimension.End;
            for (int i = 1; i < end.Column + 1; i++)
            {
                headers.Add(excelWorksheet.Cells[1, i].Value.ToString());
            }

            return headers;
        }

        private static void SetExcelHeadersAndTypes(ExcelWorksheet excelWorksheet, ref PropertyInfo[] properties)
        {
            string propertyName = string.Empty;
            HeaderName headerName;

            // Headers
            for (int i = 0; i < properties.Length; i++)
            {
                propertyName = properties[i].Name;
                headerName = properties[i].GetCustomAttribute<HeaderName>();
                if (headerName != null)
                {
                    propertyName = headerName.Name;
                }
                excelWorksheet.Column(i + 1).Style.Numberformat.Format = GetExcelFormatBasedOnType(properties[i].PropertyType);
                excelWorksheet.Cells[1, i + 1].Value = propertyName;
            }
        }

        private static void SetExcelContent<T>(ExcelWorksheet excelWorksheet, List<T> list, ref PropertyInfo[] properties)
        {
            // Content
            for (int y = 0; y < list.Count; y++)
            {
                for (int x = 0; x < properties.Length; x++)
                {
                    excelWorksheet.Cells[y + 2, x + 1].Value = properties[x].GetValue(list[y]);
                }
            }
        }

        private static string GetExcelFormatBasedOnType(Type type)
        {
            string typeClass = type.Name.ToLower();
            switch (typeClass)
            {
                case "datetime":
                    return DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                case "decimal":
                    return "$###,###,##0.00";
                case "float":
                    return "0.00";
                case "int":
                    return "#";
                case "int32":
                    return "#";
                case "int64":
                    return "#";
                default:
                    return "General";
            }
        }
    }
}
