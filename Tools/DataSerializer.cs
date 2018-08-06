using DataManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace DataManagement.Tools
{
    public class DataSerializer
    {
        /// <summary>
        /// Convierte un objeto de tipo DataTable en una Lista con formato JSON proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado que contiene una lista en formato JSON.</returns>
        public static string ConvertDataTableToJsonListOfType<T>(DataTable dataTable) where T : new()
        {
            return JsonConvert.SerializeObject(ConvertDataTableToListOfType<T>(dataTable), Formatting.None);
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable en formato JSON proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado en formato JSON.</returns>
        public static string ConvertDataTableToJsonObjectOfType<T>(DataTable dataTable) where T : new()
        {
            return JsonConvert.SerializeObject(ConvertDataTableToObjectOfType<T>(dataTable), Formatting.None);
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable en una Lista con formato XML proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado que contiene una lista en formato XML.</returns>
        public static string ConvertDataTableToXmlListOfType<T>(DataTable dataTable) where T : new()
        {
            return ConvertObjectOfTypeToXml(ConvertDataTableToListOfType<T>(dataTable));
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable en formato XML proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string ConvertDataTableToXmlObjectOfType<T>(DataTable dataTable) where T : new()
        {
            return ConvertObjectOfTypeToXml(ConvertDataTableToObjectOfType<T>(dataTable));
        }

        /// <summary>
        /// Convierte un objeto de tipo List<typeparamref name="T"/> en formato XML.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="list">Lista a deserializar</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string ConvertListOfTypeToXml<T>(List<T> list)
        {
            return ConvertObjectOfTypeToXml(list);
        }

        /// <summary>
        /// Convierte un objeto del tipo <typeparamref name="T"/> en formato XML.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="obj">Objeto a deserializar.</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string ConvertObjectOfTypeToXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, obj);
                return textWriter.ToString();
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un Objeto del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto ya convertido al tipo <typeparamref name="T"/>.</returns>
        public static T ConvertDataTableToObjectOfType<T>(DataTable dataTable) where T : new()
        {
            T newObject = new T();
            if (dataTable.Rows.Count > 0)
            {
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, ConvertStringToType(dataTable.Rows[0][propertyInfo.Name].ToString(), propertyInfo.PropertyType));
                    }
                }
            }
            else
            {
                return default;
            }

            return newObject;
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un Diccionario con el tipo de la llave <typeparamref name="TKey"/> y valor <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TKey">El tipo que se usara como llave.</typeparam>
        /// <typeparam name="TValue">El tipo que se usara como valor.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <param name="keyName">El nombre de la columna dentro del objeto DataTable.Columns que se usara como Llave.</param>
        /// <param name="valueName">El nombre de la columna dentro del objeto DataTable.Columns que se usara como Valor.</param>
        /// <returns>Regresa un nuevo Diccionario alimentado con los valores proporcionados.</returns>
        public static Dictionary<TKey, TValue> ConvertDataTableToDictionary<TKey, TValue>(DataTable dataTable, string keyName, string valueName)
        {
            if (dataTable.Columns.Count != 2)
            {
                throw new Exception("Esta funcion requiere 2 columnas en el objeto DataTable.");
            }

            Dictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>();
            if (dataTable.Rows.Count > 0)
            {
                TKey key;
                TValue value;

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    key = (TKey)ConvertStringToType(dataTable.Rows[i][keyName].ToString(), typeof(TKey));
                    value = (TValue)ConvertStringToType(dataTable.Rows[i][valueName].ToString(), typeof(TValue));

                    newDictionary.Add(key, value);
                }
            }

            return newDictionary;
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a una Lista del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa una nueva Lista del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            List<T> newList = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                T newObject = new T();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, ConvertStringToType(row[propertyInfo.Name].ToString(), propertyInfo.PropertyType));
                    }
                }
                newList.Add(newObject);
            }

            return newList;
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un Diccionario del tipo <typeparamref name="T"/>. Requiere que el tipo sea una clase que herede de la clase abstracta Main del namespace DataManagement.BO ya que utilizara la propiedad Id de la clase como Llave.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo Diccionario del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static Dictionary<Guid, T> ConvertDataTableToDictionaryOfType<T>(DataTable dataTable) where T : new()
        {
            Dictionary<Guid, T> newDictionary = new Dictionary<Guid, T>();
            foreach (DataRow row in dataTable.Rows)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                T newObject = new T();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, ConvertStringToType(row[propertyInfo.Name].ToString(), propertyInfo.PropertyType));
                    }
                }
                newDictionary.Add((newObject as Main).Id.GetValueOrDefault(), newObject);
            }

            return newDictionary;
        }

        /// <summary>
        /// Convierte un objeto de tipo List<typeparamref name="T"/> a un objeto de tipo Datatable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="list">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static DataTable ConvertListToDataTableOfType<T>(List<T> list)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Type type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in list)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["id"] };
            return dataTable;
        }

        /// <summary>
        /// Convierte un objeto de tipo String al tipo proporcionado.
        /// </summary>
        /// <param name="value">Cadena string a convertir.</param>
        /// <param name="targetType">El tipo objetivo para la conversion.</param>
        /// <returns>Regresa un nuevo Objeto ya convertido.</returns>
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
            switch (underlyingType.Name)
            {
                case "Guid":
                    return Guid.Parse(value);
                case "String":
                    return value;
                default:
                    return Convert.ChangeType(value, underlyingType);
            }
        }

        /// <summary>
        /// Coloca los parametros proporcionados en un objeto de tipo <typeparamref name="T"/> donde el nombre del parametro coincida con el de la propiedad.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para el nuevo Objeto.</typeparam>
        /// <param name="parameters">Array del objeto Parameter que contiene la informacion a colocar.</param>
        /// <returns>Regresa un nuevo objeto del tipo <typeparamref name="T"/> ya con las propiedades correspondientes alimentadas.</returns>
        public static T SetParametersInObject<T>(Parameter[] parameters) where T : new()
        {
            T newObj = new T();

            (newObj as Main).Id = null;

            foreach (Parameter data in parameters)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(data.Name);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(newObj, data.Value);
                }
            }

            return newObj;
        }
    }
}
