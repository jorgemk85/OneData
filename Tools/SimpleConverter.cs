using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataManagement.Tools
{
    public class SimpleConverter
    {
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

        public static Object MySqlParameterToObject(MySqlParameter parameter)
        {
            return parameter.Value;
        }

        public static Object MSSqlParameterToObject(SqlParameter parameter)
        {
            return parameter.Value;
        }

        public static List<Object> MySqlParameterCollectionToList(MySqlParameterCollection parameters)
        {
            List<Object> objects = new List<object>();

            foreach (MySqlParameter parameter in parameters)
            {
                objects.Add(MySqlParameterToObject(parameter));
            }
            return objects;
        }

        public static List<Object> MsSqlParameterCollectionToList(SqlParameterCollection parameters)
        {
            List<Object> objects = new List<object>();

            foreach (SqlParameter parameter in parameters)
            {
                objects.Add(MSSqlParameterToObject(parameter));
            }
            return objects;
        }
    }
}
