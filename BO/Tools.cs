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
                try
                {
                    if (propertyInfo.Name != "DataBaseTableName")
                    {
                        propertyInfo.SetValue(newObject, dataTable.Rows[0].Field<object>(propertyInfo.Name));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return newObject;
        }

        public static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            List<T> newObjectList = new List<T>();
            T newObject;

            foreach (DataRow row in dataTable.Rows)
            {
                newObject = new T();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    try
                    {
                        if (propertyInfo.Name != "DataBaseTableName")
                        {
                            propertyInfo.SetValue(newObject, row.Field<object>(propertyInfo.Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                newObjectList.Add(newObject);
            }

            return newObjectList;
        }

        public static Dictionary<Guid, T> ConvertDataTableToDictionaryOfType<T>(DataTable dataTable) where T : new()
        {
            Dictionary<Guid, T> newObjectDictionary = new Dictionary<Guid, T>();
            T newObject;

            foreach (DataRow row in dataTable.Rows)
            {
                newObject = new T();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    try
                    {
                        // Si la propiedad de la Clase no aparece como Columna en la base de datos, marca error, pero continua por que hay propiedades que son de uso interno exclusivamente.
                        propertyInfo.SetValue(newObject, row.Field<object>(propertyInfo.Name));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                newObjectDictionary.Add((newObject as Main).Id.GetValueOrDefault(), newObject);
            }

            return newObjectDictionary;
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
