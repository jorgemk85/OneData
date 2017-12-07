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
            T newObject;

            newObject = new T();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (dataTable.Columns.Contains(propertyInfo.Name))
                {
                    propertyInfo.SetValue(newObject, dataTable.Rows[0].Field<object>(propertyInfo.Name));
                }
            }

            return newObject;
        }

        public static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            List<T> newList = new List<T>();
            T newObject;

            foreach (DataRow row in dataTable.Rows)
            {
                newObject = new T();
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
            T newObject;

            foreach (DataRow row in dataTable.Rows)
            {
                newObject = new T();
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
            int newValue;

            if (!int.TryParse(value, out newValue))
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
            int newValue;

            if (!int.TryParse(value, out newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static Int64? StringToInt64(string value, bool nullable)
        {
            Int64 newValue;

            if (!Int64.TryParse(value, out newValue))
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
            Int64 newValue;

            if (!Int64.TryParse(value, out newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static decimal? StringToDecimal(string value, bool nullable)
        {
            decimal newValue;

            if (!Decimal.TryParse(value, out newValue))
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
            decimal newValue;

            if (!Decimal.TryParse(value, out newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static Guid StringToGuid(string value)
        {
            Guid newValue;

            if (!Guid.TryParse(value, out newValue))
            {
                newValue = new Guid();
            }

            return newValue;
        }

        public static List<T> AgruparLista<T>(List<T> fullList, Func<T, object> groupBy)
        {
            List<T> groupList = fullList.GroupBy(groupBy).ToDictionary(groupo => groupo.Key, groupo => groupo.First()).Values.ToList();

            return groupList;
        }
    }
}
