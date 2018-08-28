using DataManagement.Interfaces;
using DataManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
        public static string SerializeDataTableToJsonListOfType<T>(DataTable dataTable) where T : new()
        {
            return JsonConvert.SerializeObject(ConvertDataTableToListOfType<T>(dataTable), Formatting.None);
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable en formato JSON proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado en formato JSON.</returns>
        public static string SerializeDataTableToJsonObjectOfType<T>(DataTable dataTable) where T : new()
        {
            return JsonConvert.SerializeObject(ConvertDataTableToObjectOfType<T>(dataTable), Formatting.None);
        }

        /// <summary>
        /// Convierte una cadena en formato JSON a un objeto de tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para la deserializacion.</typeparam>
        /// <param name="json">La cadena JSON a convertir.</param>
        /// <returns>Regresa un objeto ya procesado del tipo proporcionado.</returns>
        public static T DeserializeJsonToObjectOfType<T>(string json) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Convierte una cadena en formato JSON a una lista de objetos de tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para la deserializacion.</typeparam>
        /// <param name="json">La cadena JSON a convertir.</param>
        /// <returns>Regresa una lista ya con los objetos procesados con del tipo proporcionado.</returns>
        public static List<T> DeserializeJsonToListOfType<T>(string json) where T : new()
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable en una Lista con formato XML proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado que contiene una lista en formato XML.</returns>
        public static string SerializeDataTableToXmlListOfType<T>(DataTable dataTable) where T : new()
        {
            return SerializeObjectOfTypeToXml(ConvertDataTableToListOfType<T>(dataTable));
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable en formato XML proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string SerializeDataTableToXmlObjectOfType<T>(DataTable dataTable) where T : new()
        {
            return SerializeObjectOfTypeToXml(ConvertDataTableToObjectOfType<T>(dataTable));
        }

        /// <summary>
        /// Convierte un objeto de tipo IEnumerable<typeparamref name="T"/> en formato XML.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="list">Arreglo a deserializar</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string SerializeIEnumerableOfTypeToXml<T>(IEnumerable<T> list)
        {
            return SerializeObjectOfTypeToXml(list);
        }

        /// <summary>
        /// Convierte un objeto del tipo <typeparamref name="T"/> en formato XML.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="obj">Objeto a deserializar.</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string SerializeObjectOfTypeToXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, obj);
                return textWriter.ToString();
            }
        }

        /// <summary>
        /// Convierte la primer fila DataRow dentro del DataTable a un Objeto del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto ya convertido al tipo <typeparamref name="T"/>.</returns>
        public static T ConvertDataTableToObjectOfType<T>(DataTable dataTable) where T : new()
        {
            if (dataTable.Rows.Count > 0)
            {
                T newObject = new T();
                foreach (PropertyInfo property in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                    {
                        property.SetValue(newObject, SimpleConverter.ConvertStringToType(dataTable.Rows[0][property.Name].ToString(), property.PropertyType));
                    }
                }
                return newObject;
            }
            else
            {
                throw new ArgumentException(string.Format("No se puede convetir el DataTable proporcionado al objeto de tipo '{0}'. No hay filas en el DataTable.", typeof(T).FullName));
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un Diccionario con el tipo de la llave <typeparamref name="TKey"/> y valor <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TKey">El tipo que se usara como Llave.</typeparam>
        /// <typeparam name="TValue">El tipo que se usara como Valor.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <param name="keyName">El nombre de la columna dentro del objeto DataTable.Columns que se usara como Llave.</param>
        /// <param name="valueName">El nombre de la columna dentro del objeto DataTable.Columns que se usara como Valor.</param>
        /// <returns>Regresa un nuevo Diccionario alimentado con los valores proporcionados.</returns>
        public static Dictionary<TKey, TValue> ConvertDataTableToDictionary<TKey, TValue>(DataTable dataTable, string keyName, string valueName)
        {
            if (dataTable.Columns.Count < 2)
            {
                throw new ArgumentException("Esta funcion requiere 2 columnas en el objeto DataTable.");
            }

            if (dataTable.Rows.Count > 0)
            {
                Dictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>();
                TKey key;
                TValue value;

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    key = (TKey)SimpleConverter.ConvertStringToType(dataTable.Rows[i][keyName].ToString(), typeof(TKey));
                    value = (TValue)SimpleConverter.ConvertStringToType(dataTable.Rows[i][valueName].ToString(), typeof(TValue));

                    newDictionary.Add(key, value);
                }
                return newDictionary;
            }
            else
            {
                throw new ArgumentException(string.Format("No se puede convetir el DataTable proporcionado a un Diccionario<{0},{1}>. No hay filas en el DataTable.", typeof(TKey).FullName, typeof(TValue).FullName));
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un Diccionario con el tipo de la llave <typeparamref name="TKey"/> y el objeto como valor.
        /// </summary>
        /// <typeparam name="TKey">El tipo que se usara como llave.</typeparam>
        /// <typeparam name="T">El tipo que se usara como objeto para el valor del diccionario.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <param name="keyName">El nombre de la columna dentro del objeto DataTable.Columns que se usara como Llave.</param>
        /// <returns>Regresa un nuevo Diccionario alimentado con los valores proporcionados.</returns>
        public static Dictionary<TKey, T> ConvertDataTableToDictionary<TKey, T>(DataTable dataTable, string keyName) where T : new()
        {
            if (dataTable.Rows.Count > 0)
            {
                Dictionary<TKey, T> newDictionary = new Dictionary<TKey, T>();
                foreach (DataRow row in dataTable.Rows)
                {
                    T newObject = new T();
                    PropertyInfo[] properties = typeof(T).GetProperties();
                    TKey key = (TKey)SimpleConverter.ConvertStringToType(row[keyName].ToString(), typeof(TKey));

                    foreach (PropertyInfo property in properties)
                    {
                        if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                        {
                            property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                        }
                    }
                    newDictionary.Add(key, newObject);
                }
                return newDictionary;
            }
            else
            {
                throw new ArgumentException(string.Format("No se puede convetir el DataTable proporcionado a un Diccionario<{0},{1}>. No hay filas en el DataTable.", typeof(TKey).FullName, typeof(T).FullName));
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a una Lista del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa una nueva Lista del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            if (dataTable.Rows.Count > 0)
            {
                List<T> newList = new List<T>();

                foreach (DataRow row in dataTable.Rows)
                {
                    PropertyInfo[] properties = typeof(T).GetProperties();
                    T newObject = new T();
                    foreach (PropertyInfo property in properties)
                    {
                        if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                        {
                            property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                        }
                    }
                    newList.Add(newObject);
                }

                return newList;
            }
            else
            {
                throw new ArgumentException(string.Format("No se puede convetir el DataTable proporcionado a una Lista<{0}>. No hay filas en el DataTable.", typeof(T).FullName));
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un HashSet del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo HashSet del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static HashSet<T> ConvertDataTableToHashSetOfType<T>(DataTable dataTable) where T : new()
        {
            if (dataTable.Rows.Count > 0)
            {
                HashSet<T> newSet = new HashSet<T>();

                foreach (DataRow row in dataTable.Rows)
                {
                    PropertyInfo[] properties = typeof(T).GetProperties();
                    T newObject = new T();
                    foreach (PropertyInfo property in properties)
                    {
                        if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                        {
                            property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                        }
                    }
                    newSet.Add(newObject);
                }

                return newSet;
            }
            else
            {
                throw new ArgumentException(string.Format("No se puede convetir el DataTable proporcionado a un HashSet<{0}>. No hay filas en el DataTable.", typeof(T).FullName));
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo DataTable a un Diccionario del tipo <typeparamref name="T"/>. Requiere que el tipo sea una clase que herede de la clase abstracta Main del namespace DataManagement.BO ya que utilizara la propiedad Id de la clase como Llave.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo Diccionario del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static Dictionary<TKey, T> ConvertDataTableToDictionaryOfType<TKey, T>(DataTable dataTable) where T : Cope<T, TKey>, new() where TKey : struct
        {
            if (dataTable.Rows.Count > 0)
            {
                Dictionary<TKey, T> newDictionary = new Dictionary<TKey, T>();
                foreach (DataRow row in dataTable.Rows)
                {
                    T newObject = new T();
                    PropertyInfo[] properties = typeof(T).GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                        {
                            property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                        }
                    }
                    newDictionary.Add(newObject.Id.GetValueOrDefault(), newObject);
                }

                return newDictionary;
            }
            else
            {
                throw new ArgumentException(string.Format("No se puede convetir el DataTable proporcionado a un Diccionario<{0},{1}>. No hay filas en el DataTable.", typeof(TKey).FullName, typeof(T).FullName));
            }
        }

        /// <summary>
        /// Convierte un objeto de tipo IEnumerable <typeparamref name="T"/> a un objeto de tipo DataTable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="list">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static DataTable ConvertIEnumerableToDataTableOfGenericType<T>(IEnumerable<T> list)
        {
            return ConvertIEnumerableToDataTable(list);
        }

        /// <summary>
        /// Convierte un objeto de tipo IEnumerable <typeparamref name="T"/> que implementa la clase Cope a un objeto de tipo Datatable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para convertir.</typeparam>
        /// <typeparam name="TKey">El tipo de la llave del tipo <typeparamref name="T"/> referencia para convertir.</typeparam>
        /// <param name="list">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static DataTable ConvertIEnumerableToDataTableOfType<T, TKey>(IEnumerable<T> list) where T : Cope<T, TKey>, new() where TKey : struct
        {
            DataTable dataTable = ConvertIEnumerableToDataTable(list);
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["Id"] };
            return dataTable;
        }

        private static DataTable ConvertIEnumerableToDataTable<T>(IEnumerable<T> list)
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

            return dataTable;
        }

        /// <summary>
        /// Convierte un objeto de de tipo <typeparamref name="T"/> a un objeto de tipo Datatable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para convertir.</typeparam>
        /// <param name="obj">El objeto a convertir.</param>
        /// <returns>Regresa un nuevo DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static DataTable ConvertObjectOfTypeToDataTable<T>(T obj)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Type type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }

            var values = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(obj, null);
            }

            dataTable.Rows.Add(values);
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["id"] };

            return dataTable;
        }

        /// <summary>
        /// Coloca los parametros proporcionados en un objeto de tipo <typeparamref name="T"/> donde el nombre del parametro coincida con el de la propiedad.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para el nuevo Objeto.</typeparam>
        /// <param name="parameters">Array del objeto Parameter que contiene la informacion a colocar.</param>
        /// <returns>Regresa un nuevo objeto del tipo <typeparamref name="T"/> ya con las propiedades correspondientes alimentadas.</returns>
        internal static T SetParametersInObject<T, TKey>(Parameter[] parameters) where T : Cope<T, TKey>, new() where TKey : struct
        {
            T newObj = new T
            {
                Id = null
            };

            foreach (Parameter data in parameters)
            {
                PropertyInfo property = typeof(T).GetProperty(data.Name);
                if (property != null)
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(newObj, data.Value);
                    }
                }
            }

            return newObj;
        }
    }
}
