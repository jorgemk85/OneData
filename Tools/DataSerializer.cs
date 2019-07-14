using Newtonsoft.Json;
using OneData.Attributes;
using OneData.DAO;
using OneData.Enums;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace OneData.Tools
{
    public class DataSerializer
    {
        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable en una Lista con formato JSON proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado que contiene una lista en formato JSON.</returns>
        public static string SerializeDataTableToJsonListOfType<T>(System.Data.DataTable dataTable) where T : new()
        {
            return JsonConvert.SerializeObject(ConvertDataTableToListOfType<T>(dataTable), Formatting.None);
        }

        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable en formato JSON proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado en formato JSON.</returns>
        public static string SerializeDataTableToJsonObjectOfType<T>(System.Data.DataTable dataTable) where T : new()
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
        /// Convierte un objeto de tipo System.Data.DataTable en una Lista con formato XML proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado que contiene una lista en formato XML.</returns>
        public static string SerializeDataTableToXmlListOfType<T>(System.Data.DataTable dataTable) where T : new()
        {
            return SerializeObjectOfTypeToXml(ConvertDataTableToListOfType<T>(dataTable));
        }

        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable en formato XML proporcionando un tipo <typeparamref name="T"/> para la serializacion.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string SerializeDataTableToXmlObjectOfType<T>(System.Data.DataTable dataTable) where T : new()
        {
            return SerializeObjectOfTypeToXml(ConvertDataTableToObjectOfType<T>(dataTable));
        }

        /// <summary>
        /// Convierte un objeto de tipo IEnumerable<typeparamref name="T"/> en formato XML.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para deserializar.</typeparam>
        /// <param name="list">Arreglo a deserializar</param>
        /// <returns>Regresa un objeto string ya procesado en formato XML.</returns>
        public static string SerializeIEnumerableOfTypeToXml<T>(List<T> list)
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

        internal static MassiveOperationParameter GenerateCompatibleMassiveOperationXML<T>(IEnumerable<T> list, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            MassiveOperationParameter massiveOperationParameter = new MassiveOperationParameter();
            StringBuilder builder = new StringBuilder();

            builder.Append("<columns>\n");
            if (transactionType == TransactionTypes.DeleteMassive)
            {
                builder.Append("  <column>\n");
                builder.Append($"      <name>{Cope<T>.ModelComposition.PrimaryKeyProperty.Name}</name>\n");
                builder.Append("  </column>\n");
            }
            else
            {
                foreach (KeyValuePair<string, OneProperty> property in Cope<T>.ModelComposition.FilteredProperties)
                {
                    builder.Append("  <column>\n");
                    builder.Append($"      <name>{property.Value.Name}</name>\n");
                    builder.Append("  </column>\n");
                }
            }
            builder.Append("</columns>");
            massiveOperationParameter.XmlNames = builder.ToString();

            builder.Clear();

            builder.Append("<objects>\n");
            if (transactionType == TransactionTypes.DeleteMassive)
            {
                foreach (T obj in list)
                {
                    builder.Append("  <object>\n");
                    // Si es Numero o Boolean no agrega comillas sencillas, de lo contrario se las pone.
                    if (long.TryParse(Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj).ToString(), out long n) || Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj) is bool)
                    {
                        builder.Append($"     <{Cope<T>.ModelComposition.PrimaryKeyProperty.Name}>{Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)}</{Cope<T>.ModelComposition.PrimaryKeyProperty.Name}>\n");
                    }
                    else
                    {
                        builder.Append($"     <{Cope<T>.ModelComposition.PrimaryKeyProperty.Name}>'{Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)}'</{Cope<T>.ModelComposition.PrimaryKeyProperty.Name}>\n");
                    }
                    builder.Append("  </object>\n");
                }
            }
            else
            {
                foreach (T obj in list)
                {
                    builder.Append("  <object>\n");
                    foreach (KeyValuePair<string, OneProperty> property in Cope<T>.ModelComposition.FilteredProperties)
                    {
                        if (property.Value.GetValue(obj) == null)
                        {
                            builder.Append($"     <{property.Value.Name}>{DBNull.Value}</{property.Value.Name}>\n");
                        }
                        else
                        {
                            // Si es Numero o Boolean no agrega comillas sencillas, de lo contrario se las pone.
                            if (long.TryParse(property.Value.GetValue(obj).ToString(), out long n) || property.Value.GetValue(obj) is bool)
                            {
                                builder.Append($"     <{property.Value.Name}>{property.Value.GetValue(obj)}</{property.Value.Name}>\n");
                            }
                            else
                            {
                                if (property.Value.GetValue(obj) is DateTime)
                                {
                                    builder.Append($"     <{property.Value.Name}>'{((DateTime)property.Value.GetValue(obj)).ToString("yyyy-MM-dd H:mm:ss")}'</{property.Value.Name}>\n");
                                }
                                else
                                {
                                    builder.Append($"     <{property.Value.Name}>'{property.Value.GetValue(obj)}'</{property.Value.Name}>\n");
                                }
                            }
                        }
                    }
                    builder.Append("  </object>\n");
                }
            }

            builder.Append("</objects>");

            massiveOperationParameter.XmlValues = builder.ToString();
            switch (transactionType)
            {
                case TransactionTypes.InsertMassive:
                    massiveOperationParameter.ProcedureName = Manager.ConnectionType == ConnectionTypes.MySQL ? $"`{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.InsertSuffix}`" : $"[{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.InsertSuffix}]";
                    break;
                case TransactionTypes.UpdateMassive:
                    massiveOperationParameter.ProcedureName = Manager.ConnectionType == ConnectionTypes.MySQL ? $"`{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.UpdateSuffix}`" : $"[{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.UpdateSuffix}]";
                    break;
                case TransactionTypes.DeleteMassive:
                    massiveOperationParameter.ProcedureName = Manager.ConnectionType == ConnectionTypes.MySQL ? $"`{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.DeleteSuffix}`" : $"[{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.DeleteSuffix}]";
                    break;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con la funcion {nameof(GenerateCompatibleMassiveOperationXML)}.");
            }

            return massiveOperationParameter;
        }

        /// <summary>
        /// Convierte la primer fila DataRow dentro del System.Data.DataTable a un Objeto del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un objeto ya convertido al tipo <typeparamref name="T"/>.</returns>
        public static T ConvertDataTableToObjectOfType<T>(System.Data.DataTable dataTable) where T : new()
        {
            T newObject = new T();
            if (dataTable.Rows.Count > 0)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                Dictionary<string, HeaderName> headers = GetCustomAttributesFromPropertiesInClass<HeaderName, T>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.CanWrite)
                        {
                            if (headers.TryGetValue(property.Name, out HeaderName header))
                            {
                                if (header.Name.Equals(column.ColumnName))
                                {
                                    property.SetValue(newObject, SimpleConverter.ConvertStringToType(dataTable.Rows[0][header.Name].ToString(), property.PropertyType));
                                    break;
                                }
                            }
                            else if (column.ColumnName.Equals(property.Name))
                            {
                                property.SetValue(newObject, SimpleConverter.ConvertStringToType(dataTable.Rows[0][property.Name].ToString(), property.PropertyType));
                                break;
                            }
                        }
                    }
                }
            }

            return newObject;
        }

        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable a un Diccionario con el tipo de la llave <typeparamref name="TKey"/> y valor <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TKey">El tipo que se usara como Llave.</typeparam>
        /// <typeparam name="TValue">El tipo que se usara como Valor.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <param name="keyName">El nombre de la columna dentro del objeto System.Data.DataTable.Columns que se usara como Llave.</param>
        /// <param name="valueName">El nombre de la columna dentro del objeto System.Data.DataTable.Columns que se usara como Valor.</param>
        /// <returns>Regresa un nuevo Diccionario alimentado con los valores proporcionados.</returns>
        public static Dictionary<TKey, TValue> ConvertDataTableToDictionary<TKey, TValue>(System.Data.DataTable dataTable, string keyName, string valueName)
        {
            if (dataTable.Columns.Count < 2)
            {
                throw new ArgumentException("Esta funcion requiere 2 columnas en el objeto System.Data.DataTable.");
            }

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

        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable a un Diccionario con el tipo de la llave <typeparamref name="TKey"/> y el objeto como valor.
        /// </summary>
        /// <typeparam name="TKey">El tipo que se usara como llave.</typeparam>
        /// <typeparam name="T">El tipo que se usara como objeto para el valor del diccionario.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <param name="keyName">El nombre de la columna dentro del objeto System.Data.DataTable.Columns que se usara como Llave.</param>
        /// <returns>Regresa un nuevo Diccionario alimentado con los valores proporcionados.</returns>
        public static Dictionary<TKey, T> ConvertDataTableToDictionary<TKey, T>(System.Data.DataTable dataTable, string keyName) where T : new()
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

        public static Dictionary<dynamic, T> ConvertDataTableToDictionaryOfType<T>(System.Data.DataTable dataTable, string keyName, Type keyType) where T : new()
        {
            Dictionary<dynamic, T> newDictionary = new Dictionary<dynamic, T>();
            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    T newObject = new T();
                    PropertyInfo[] properties = typeof(T).GetProperties();
                    dynamic key = SimpleConverter.ConvertStringToType(row[keyName].ToString(), keyType);

                    foreach (PropertyInfo property in properties)
                    {
                        if (dataTable.Columns.Contains(property.Name) && property.CanWrite)
                        {
                            property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                        }
                    }
                    newDictionary.Add(key, newObject);
                }
            }
            return newDictionary;
        }

        public static Dictionary<dynamic, T> ConvertQueryableToDictionaryOfType<T>(IQueryable queryable, string keyName, Type keyType) where T : Cope<T>, IManageable, new()
        {
            Dictionary<dynamic, T> newDictionary = new Dictionary<dynamic, T>();
            if (queryable != null)
            {
                PropertyInfo primaryProperty = typeof(T).GetProperty(Cope<T>.ModelComposition.PrimaryKeyProperty.Name);
                foreach (T item in queryable)
                {
                    dynamic key = primaryProperty.GetValue(item);
                    newDictionary.Add(key, item);
                }
            }
            return newDictionary;
        }

        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable a una Lista del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa una nueva Lista del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static List<T> ConvertDataTableToListOfType<T>(System.Data.DataTable dataTable) where T : new()
        {
            List<T> newList = new List<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            Dictionary<string, HeaderName> headers = GetCustomAttributesFromPropertiesInClass<HeaderName, T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T newObject = new T();
                foreach (DataColumn column in dataTable.Columns)
                {
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.CanWrite)
                        {
                            if (headers.TryGetValue(property.Name, out HeaderName header))
                            {
                                if (header.Name.Equals(column.ColumnName))
                                {
                                    property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[header.Name].ToString(), property.PropertyType));
                                    break;
                                }
                            }
                            else if (column.ColumnName.Equals(property.Name))
                            {
                                property.SetValue(newObject, SimpleConverter.ConvertStringToType(row[property.Name].ToString(), property.PropertyType));
                                break;
                            }
                        }
                    }
                }

                newList.Add(newObject);
            }

            return newList;
        }

        public static Dictionary<string, TAttr> GetCustomAttributesFromPropertiesInClass<TAttr, TClass>() where TAttr : Attribute
        {
            Dictionary<string, TAttr> attributes = new Dictionary<string, TAttr>();
            foreach (PropertyInfo property in typeof(TClass).GetProperties())
            {
                if (property.GetCustomAttribute(typeof(TAttr)) is TAttr attribute)
                {
                    //foreach (PropertyInfo attributeProperty in typeof(TAttr).GetProperties())
                    //{
                    //    if (attributeProperty.Name.Equals(keyName))
                    //    {
                    //        keyValue = attributeProperty.GetValue(attribute) as TKey;
                    //        break;
                    //    }
                    //}
                    attributes.Add(property.Name, attribute);
                }
            }
            return attributes;
        }

        public static Hashtable ConvertDataTableToHashtable(System.Data.DataTable dataTable, string keyName)
        {
            Hashtable newHashTable = new Hashtable();
            object key;
            object value;
            if (dataTable != null)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    key = dataTable.Rows[i][keyName].ToString();
                    value = dataTable.Rows[i];

                    newHashTable.Add(key, value);
                }
            }
            return newHashTable;
        }

        public static Hashtable ConvertDataTableToHashtableOfType<T>(System.Data.DataTable dataTable) where T : Cope<T>, IManageable, new()
        {
            Hashtable newHashTable = new Hashtable();
            if (dataTable != null)
            {
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
                    newHashTable.Add(Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(newObject), newObject);
                }
            }

            return newHashTable;
        }

        public static T ConvertHashtableToObjectOfType<T>(Hashtable hashtable) where T : new()
        {
            var enumerator = hashtable.GetEnumerator();
            enumerator.MoveNext();

            return (T)enumerator.Current;
        }

        public static List<T> ConvertHashtableToListOfType<T>(Hashtable hashtable) where T : new()
        {
            List<T> newList = new List<T>();
            foreach (DictionaryEntry row in hashtable)
            {
                newList.Add((T)row.Value);
            }

            return newList;
        }

        public static T ConvertDictionaryToObjectOfType<TKey, T>(Dictionary<TKey, T> dictionary) where T : new()
        {
            var enumerator = dictionary.GetEnumerator();
            enumerator.MoveNext();

            return enumerator.Current.Value;
        }

        public static List<T> ConvertDictionaryToListOfType<TKey, T>(Dictionary<TKey, T> dictionary) where T : new()
        {
            return new List<T>(dictionary.Values);
        }

        public static IEnumerable<T> ConvertDictionaryToIEnumerableOfType<TKey, T>(Dictionary<TKey, T> dictionary) where T : new()
        {
            return dictionary.Values;
        }

        public static ICollection<object> ConvertDataTableToListOfType(System.Data.DataTable dataTable, Type target)
        {
            ICollection<object> newList = new List<object>();
            foreach (DataRow row in dataTable.Rows)
            {
                PropertyInfo[] properties = target.GetProperties();
                object newObject = Activator.CreateInstance(target);
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


        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable a un HashSet del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo HashSet del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static HashSet<T> ConvertDataTableToHashSetOfType<T>(System.Data.DataTable dataTable) where T : new()
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

        /// <summary>
        /// Convierte un objeto de tipo System.Data.DataTable a un Diccionario del tipo <typeparamref name="T"/>. Requiere que el tipo sea una clase que herede de la clase abstracta Main del namespace OneData.BO ya que utilizara la propiedad Id de la clase como Llave.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="dataTable">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo Diccionario del tipo <typeparamref name="T"/> ya con los objetos incorporados.</returns>
        public static Dictionary<TKey, T> ConvertDataTableToDictionaryOfType<TKey, T>(System.Data.DataTable dataTable) where T : Cope<T>, IManageable, new()
        {
            Dictionary<TKey, T> newDictionary = new Dictionary<TKey, T>();
            if (dataTable != null)
            {
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
                    newDictionary.Add((TKey)Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(newObject), newObject);
                }
            }

            return newDictionary;
        }

        public static Dictionary<TKey, T> ConvertIEnumerableToDictionaryOfType<TKey, T>(IEnumerable<T> list) where T : Cope<T>, IManageable, new()
        {
            Dictionary<TKey, T> newDictionary = new Dictionary<TKey, T>();
            if (list != null)
            {
                foreach (T newObject in list)
                {
                    newDictionary.Add((TKey)Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(newObject), newObject);
                }
            }

            return newDictionary;
        }

        /// <summary>
        /// Convierte un objeto de tipo IEnumerable <typeparamref name="T"/> a un objeto de tipo System.Data.DataTable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para serializar.</typeparam>
        /// <param name="list">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo System.Data.DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static System.Data.DataTable ConvertIEnumerableToDataTableOfGenericType<T>(IEnumerable<T> list)
        {
            return ConvertIEnumerableToDataTable(list);
        }

        /// <summary>
        /// Convierte un objeto de tipo IEnumerable <typeparamref name="T"/> que implementa la clase Cope a un objeto de tipo Datatable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para convertir.</typeparam>
        /// <typeparam name="TKey">El tipo de la llave del tipo <typeparamref name="T"/> referencia para convertir.</typeparam>
        /// <param name="list">El contenido a convertir.</param>
        /// <returns>Regresa un nuevo System.Data.DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static System.Data.DataTable ConvertIEnumerableToDataTableOfType<T>(IEnumerable<T> list) where T : Cope<T>, IManageable, new()
        {
            System.Data.DataTable dataTable = ConvertIEnumerableToDataTable(list);
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["Id"] };
            return dataTable;
        }

        private static System.Data.DataTable ConvertIEnumerableToDataTable<T>(IEnumerable<T> list)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable(typeof(T).Name);
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

        public static Dictionary<TKey, T> ConvertIEnumerableToDictionaryOfType<TKey, T>(IEnumerable<T> list, Expression<Func<T, TKey>> keyProperty)
        {
            var newDictionary = new Dictionary<TKey, T>();
            if (list != null)
            {
                MemberExpression expressionRequired = null;
                var body = ExpressionTools.GetExpressionBodyType(keyProperty, t => expressionRequired);
                LoopIEnumerableToDictionaryOfType(list, body.Member.Name, ref newDictionary);
            }
            return newDictionary;
        }

        public static Dictionary<TKey, T> ConvertIEnumerableToDictionaryOfType<TKey, T>(IEnumerable<T> list, string keyPropertyName)
        {
            var newDictionary = new Dictionary<TKey, T>();
            if (list != null)
            {
                LoopIEnumerableToDictionaryOfType(list, keyPropertyName, ref newDictionary);
            }
            return newDictionary;
        }

        private static void LoopIEnumerableToDictionaryOfType<TKey, T>(IEnumerable<T> list, string keyPropertyName, ref Dictionary<TKey, T> dictionary)
        {
            PropertyInfo keyProperty = typeof(T).GetProperty(keyPropertyName);
            foreach (T item in list)
            {
                TKey key = (TKey)keyProperty.GetValue(item);
                dictionary.Add(key, item);
            }
        }

        /// <summary>
        /// Convierte un objeto de de tipo <typeparamref name="T"/> a un objeto de tipo Datatable.
        /// </summary>
        /// <typeparam name="T">Tipo referencia para convertir.</typeparam>
        /// <param name="obj">El objeto a convertir.</param>
        /// <returns>Regresa un nuevo System.Data.DataTable ya con los objetos incorporados como columnas y filas.</returns>
        public static System.Data.DataTable ConvertObjectOfTypeToDataTable<T>(T obj)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable(typeof(T).Name);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Type type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
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
        internal static T SetParametersInObject<T>(Parameter[] parameters) where T : Cope<T>, IManageable, new()
        {
            T newObj = new T();

            Cope<T>.ModelComposition.PrimaryKeyProperty.SetValue(newObj, null);

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

        internal static T ConvertReaderToObjectOfType<T>(IDataReader reader, IEnumerable<OneProperty> properties) where T : Cope<T>, IManageable, new()
        {
            T newObj = new T();

            foreach (OneProperty property in properties)
            {
                property.SetValue(newObj, SimpleConverter.ConvertStringToType(reader[property.Name].ToString(), property.PropertyType));
            }

            return newObj;
        }

        internal static IEnumerable<OneProperty> GetFilteredPropertiesBasedOnList<T>(IDataReader reader) where T : Cope<T>, IManageable, new()
        {
            List<string> columns = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i).ToLower());
            }
            List<OneProperty> filteredProperties = Cope<T>.ModelComposition.ManagedProperties.Values.Where(property => columns.Contains(property.Name.ToLower())).ToList();
            filteredProperties.AddRange(Cope<T>.ModelComposition.ForeignDataProperties.Values.Where(property => columns.Contains(property.Name.ToLower())));
            return filteredProperties;
        }
    }
}
