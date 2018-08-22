using DataManagement.Exceptions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace DataManagement.Tools
{
    public class SimpleConverter
    {
        /// <summary>
        /// Convierte un objeto de tipo String al tipo proporcionado. Esta funcion acepta Nullable Types.
        /// </summary>
        /// <param name="value">Cadena string a convertir.</param>
        /// <param name="targetType">El tipo objetivo para la conversion.</param>
        /// <returns>Regresa un nuevo Objeto ya convertido al tipo deseado.</returns>
        public static object ConvertStringToType(string value, Type targetType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(targetType);

            if (underlyingType != null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
            }
            else
            {
                underlyingType = targetType;
            }

            try
            {
                switch (underlyingType.Name)
                {
                    case "Guid":
                        return Guid.Parse(value);
                    case "String":
                        return value;
                    case "Decimal":
                        return Convert.ChangeType(value.Replace("$", string.Empty), underlyingType);
                    case "Float":
                        return Convert.ChangeType(value.Replace("$", string.Empty), underlyingType);
                    case "Double":
                        return Convert.ChangeType(value.Replace("$", string.Empty), underlyingType);
                    case "DateTime":
                        try
                        {
                            if (value.EndsWith("AM") || value.EndsWith("PM"))
                            {
                                return DateTime.ParseExact(value, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            }
                            return DateTime.Parse(value);
                        }
                        catch (Exception e)
                        {
                            return DateTime.FromOADate(double.Parse(value));
                        }
                    default:
                        return Convert.ChangeType(value, underlyingType);
                }
            }
            catch (FormatException e)
            {
                throw new ConvertionFailedException(value, targetType, e);
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo string en un objeto de tipo Nullable<int>.
        /// </summary>
        /// <param name="value">Valor a convertir.</param>
        /// <param name="nullable">Especifica si el valor es nullable (int?) o no.</param>
        /// <returns>Regresa un nuevo objeto de tipo Nullable<int> ya con el valor incorporado.</returns>
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

        /// <summary>
        /// Convierte un objeto de tipo string en un objeto de tipo Nullable<Int64>.
        /// </summary>
        /// <param name="value">Valor a convertir.</param>
        /// <param name="nullable">Especifica si el valor es nullable (Int64?) o no.</param>
        /// <returns>Regresa un nuevo objeto de tipo Nullable<Int64> ya con el valor incorporado.</returns>
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

        /// <summary>
        /// Convierte un objeto de tipo string en un objeto de tipo Nullable<decimal>.
        /// </summary>
        /// <param name="value">Valor a convertir.</param>
        /// <param name="nullable">Especifica si el valor es nullable (decimal?) o no.</param>
        /// <returns>Regresa un nuevo objeto de tipo Nullable<decimal> ya con el valor incorporado.</returns>
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

        /// <summary>
        /// Convierte un objeto de tipo string a un objeto de tipo Guid.
        /// </summary>
        /// <param name="value">Valor a convertir.</param>
        /// <returns>Regresa un nuevo objeto de tipo Guid ya con el valor incorporado.</returns>
        public static Guid StringToGuid(string value)
        {
            if (!Guid.TryParse(value, out Guid newValue))
            {
                newValue = new Guid();
            }

            return newValue;
        }

        private static Object MySqlParameterToObject(MySqlParameter parameter)
        {
            return parameter.Value;
        }

        private static Object MSSqlParameterToObject(SqlParameter parameter)
        {
            return parameter.Value;
        }

        /// <summary>
        /// Convierte la coleccion contenida en un objeto de tipo MySqlParameterCollection en un objeto List<Object>.
        /// </summary>
        /// <param name="parameters">Coleccion de parametros a convertir.</param>
        /// <returns>Regresa un nuevo objeto List<Object> con los valores de la colleccion proporcionada.</returns>
        public static List<Object> MySqlParameterCollectionToList(MySqlParameterCollection parameters)
        {
            List<Object> objects = new List<object>();

            foreach (MySqlParameter parameter in parameters)
            {
                objects.Add(MySqlParameterToObject(parameter));
            }
            return objects;
        }

        /// <summary>
        /// Convierte la coleccion contenida en un objeto de tipo SqlParameterCollection en un objeto List<Object>.
        /// </summary>
        /// <param name="parameters">Coleccion de parametros a convertir.</param>
        /// <returns>Regresa un nuevo objeto List<Object> con los valores de la colleccion proporcionada.</returns>
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
