using OneData.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneData.Tools
{
    public class FileSerializer
    {
        /// <summary>
        /// Deserializa un archivo plano delimitado por un caracter en un objeto del tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullyQualifiedFileName">Nombre completo del archivo a serializar incluyendo direccion en disco.</param>
        /// <param name="separator">Caracter delimitador en el archivo.</param>
        /// <param name="fileEncoding">Codificacion utilizada en el archivo plano.</param>
        /// <returns>Regresa una nueva Lista del tipo <typeparamref name="T"/> con la informacion ya procesada del archivo plano.</returns>
        public static List<T> DeserializeFileToListOfType<T>(string fullyQualifiedFileName, char separator, Encoding fileEncoding) where T : new()
        {
            return DeserializeFile<T>(fullyQualifiedFileName, separator, fileEncoding);
        }

        /// <summary>
        /// Deserializa un archivo plano delimitado por un caracter en un objeto del tipo <typeparamref name="T"/> usando Async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullyQualifiedFileName">Nombre completo del archivo a serializar incluyendo direccion en disco.</param>
        /// <param name="separator">Caracter delimitador en el archivo.</param>
        /// <param name="fileEncoding">Codificacion utilizada en el archivo plano.</param>
        /// <returns>Regresa una nueva Lista del tipo <typeparamref name="T"/> con la informacion ya procesada del archivo plano.</returns>
        public static async Task<List<T>> DeserializeFileToListOfTypeAsync<T>(string fullyQualifiedFileName, char separator, Encoding fileEncoding) where T : new()
        {
            return await Task.Run(() => DeserializeFile<T>(fullyQualifiedFileName, separator, fileEncoding));
        }

        /// <summary>
        /// Serializa un objeto de tipo IEnumerable <typeparamref name="T"/> y lo guarda en un archivo de texto.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto.</typeparam>
        /// <param name="list">Lista de objetos de tipo <typeparamref name="T"/> que se convertira.</param>
        /// <param name="fullyQualifiedFileName">Directorio completo, incluyendo nombre de archivo y extension. Se utiliza para guardar el producto final.</param>
        public static void SerializeIEnumerableOfTypeToFile<T>(IEnumerable<T> list, string fullyQualifiedFileName, char separator)
        {
            SerializeIEnumerable(list, fullyQualifiedFileName, separator);
        }

        /// <summary>
        /// Serializa un objeto de tipo IEnumerable <typeparamref name="T"/> y lo guarda en un archivo de texto utilizando Async.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto.</typeparam>
        /// <param name="list">Lista de objetos de tipo <typeparamref name="T"/> que se convertira.</param>
        /// <param name="fullyQualifiedFileName">Directorio completo, incluyendo nombre de archivo y extension. Se utiliza para guardar el producto final.</param>
        public static async void SerializeIEnumerableOfTypeToFileAsync<T>(IEnumerable<T> list, string fullyQualifiedFileName, char separator)
        {
            await Task.Run(() => SerializeIEnumerable(list, fullyQualifiedFileName, separator));
        }

        private static void SerializeIEnumerable<T>(IEnumerable<T> list, string fullyQualifiedFileName, char separator)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(fullyQualifiedFileName))
                {
                    // Primero escribimos los headers.
                    string headerName = string.Empty;
                    StringBuilder headerBuilder = new StringBuilder();
                    foreach (PropertyInfo property in typeof(T).GetProperties())
                    {
                        headerName = property.Name;
                        if (property.GetCustomAttribute<HeaderName>() != null)
                        {
                            headerName = property.GetCustomAttribute<HeaderName>().Name;
                        }
                        headerBuilder.AppendFormat("{0}{1}", headerName, separator);
                    }
                    streamWriter.WriteLine(headerBuilder.ToString().Remove(headerBuilder.ToString().Length - 1));

                    // Ahora escribimos la lista en el archivo, linea por linea.
                    StringBuilder lineBuilder = new StringBuilder();
                    foreach (T item in list)
                    {
                        lineBuilder.Clear();
                        foreach (PropertyInfo property in typeof(T).GetProperties())
                        {
                            lineBuilder.AppendFormat("{0}{1}", property.GetValue(item), separator);
                        }
                        streamWriter.WriteLine(lineBuilder.ToString().Remove(lineBuilder.ToString().Length - 1));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static List<T> DeserializeFile<T>(string fullyQualifiedFileName, char separator, Encoding fileEncoding) where T : new()
        {
            try
            {
                List<T> newList = new List<T>();
                using (StreamReader sr = new StreamReader(fullyQualifiedFileName, fileEncoding))
                {
                    // Primero leemos la primer linea y la guardamos como los headers.
                    List<string> headers = GetHeadersFromLine(sr.ReadLine(), separator);

                    String line;
                    T newObj;
                    while ((line = sr.ReadLine()) != null)
                    {
                        newObj = DeserializeLineToObjectOfType<T>(headers, line, separator);
                        if (newObj != null)
                        {
                            newList.Add(newObj);
                        }
                    }
                }
                return newList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static List<string> GetHeadersFromLine(string line, char separator)
        {
            List<string> headers = new List<string>();
            string[] lineSplit = line.Split(separator);

            for (int i = 0; i < lineSplit.Length; i++)
            {
                headers.Add(lineSplit[i]);
            }

            return headers;
        }

        private static T DeserializeLineToObjectOfType<T>(List<string> headers, string line, char separator) where T : new()
        {
            string[] lineSplit = line.Split(separator);
            Dictionary<string, string> columnsNames = new Dictionary<string, string>();
            T newObj;

            for (int i = 0; i < lineSplit.Length; i++)
            {
                columnsNames.Add(headers[i], lineSplit[i]);
            }

            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                newObj = new T();

                foreach (string header in headers)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        HeaderName headerNameAttribute = null;
                        string propertyName = properties[i].Name;

                        headerNameAttribute = properties[i].GetCustomAttribute<HeaderName>();
                        if (headerNameAttribute != null)
                        {
                            propertyName = headerNameAttribute.Name;
                        }

                        if (propertyName.Equals(header))
                        {
                            SetValueInProperty(columnsNames, properties[i], headerNameAttribute, header, newObj);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return newObj;
        }

        private static void SetValueInProperty<T>(Dictionary<string, string> columns, PropertyInfo property, HeaderName headerNameAttribute, string header, T newObj)
        {
            if (!property.CanWrite)
            {
                return;
            }

            if (columns.ContainsKey(header))
            {
                property.SetValue(newObj, SimpleConverter.ConvertStringToType(columns[header], property.PropertyType));
            }
            else
            {
                if (headerNameAttribute == null)
                {
                    throw new Exception(string.Format("No se encontro algun dato para el encabezado {0} en el archivo proporcionado.", header));
                }
                else
                {
                    if (headerNameAttribute.Important)
                    {
                        throw new Exception(string.Format("No se encontro algun dato para el encabezado {0} en el archivo proporcionado.", header));
                    }
                }
            }
        }
    }
}
