using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DataAccess.BO
{
    public class Tools
    {
        public static T ConvertDataTableToObjectOfType<T>(DataTable dataTable) where T : new()
        {
            T newObject = new T();
            if (dataTable.Rows.Count > 0)
            {
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, dataTable.Rows[0].Field<object>(propertyInfo.Name));
                    }
                }
            }
            else
            {
                return default(T);
            }

            return newObject;
        }

        public static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            List<T> newList = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                T newObject = new T();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, row.Field<object>(propertyInfo.Name));
                    }
                }
                newList.Add(newObject);
            }

            return newList;
        }

        public static Dictionary<Guid, T> ConvertDataTableToDictionaryOfType<T>(DataTable dataTable) where T : new()
        {
            Dictionary<Guid, T> newDictionary = new Dictionary<Guid, T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T newObject = new T();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, row.Field<object>(propertyInfo.Name));
                    }
                }
                newDictionary.Add((newObject as Main).Id.GetValueOrDefault(), newObject);
            }

            return newDictionary;
        }

        public static DataTable ConvertListToDataTableOfType<T>(List<T> list)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                Type type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in list)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable FillDataTable(MySqlDataReader dr)
        {
            DataTable dataTable = new DataTable();
            object[] values = new object[dr.FieldCount];
            for (int i = 0; i < dr.FieldCount; i++)
            {
                dataTable.Columns.Add(dr.GetName(i));
            }
            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    values[i] = dr[i];
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static int? StringToInteger(string value, bool nullable)
        {
            if (!int.TryParse(value, out int newValue))
            {
                if (!nullable)
                {
                    newValue = 0;
                }
                else
                {
                    return null;
                }
            }

            return newValue;
        }

        public static int StringToInteger(string value)
        {
            if (!int.TryParse(value, out int newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static Int64? StringToInt64(string value, bool nullable)
        {
            if (!Int64.TryParse(value, out long newValue))
            {
                if (!nullable)
                {
                    newValue = 0;
                }
                else
                {
                    return null;
                }
            }

            return newValue;
        }

        public static Int64 StringToInt64(string value)
        {
            if (!Int64.TryParse(value, out long newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static decimal? StringToDecimal(string value, bool nullable)
        {
            if (!Decimal.TryParse(value, out decimal newValue))
            {
                if (!nullable)
                {
                    newValue = 0;
                }
                else
                {
                    return null;
                }
            }

            return newValue;
        }

        public static decimal StringToDecimal(string value)
        {
            if (!Decimal.TryParse(value, out decimal newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static Guid StringToGuid(string value)
        {
            if (!Guid.TryParse(value, out Guid newValue))
            {
                newValue = new Guid();
            }

            return newValue;
        }
    }
}
