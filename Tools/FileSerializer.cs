using DataManagement.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Tools
{
    public class FileSerializer
    {
        /// <summary>
        /// Serializa un archivo plano delimitado por un caracter en un objeto del tipo <typeparamref name="T"/>.
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
        /// Serializa un archivo plano delimitado por un caracter en un objeto del tipo <typeparamref name="T"/> usando Async.
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

        public static void SerializeListOfTypeToFile<T>(List<T> list, string fullyQualifiedFileName, char separator)
        {
            SerializeList(list, fullyQualifiedFileName, separator);
        }

        public static async void SerializeListOfTypeToFileAsync<T>(List<T> list, string fullyQualifiedFileName, char separator)
        {
            await Task.Run(() => SerializeList(list, fullyQualifiedFileName, separator));
        }

        private static void SerializeList<T>(List<T> list, string fullyQualifiedFileName, char separator)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fullyQualifiedFileName))
                {
                    // Primero escribimos los headers.
                    string headers = string.Empty;
                    string headerName = string.Empty;
                    foreach (PropertyInfo property in typeof(T).GetProperties())
                    {
                        headerName = property.Name;
                        if (property.GetCustomAttribute<HeaderName>() != null)
                        {
                            headerName = property.GetCustomAttribute<HeaderName>().Name;
                        }
                        headers += string.Format("{0}{1}", headerName, separator);
                    }
                    sw.WriteLine(headers.Remove(headers.Length - 1));

                    // Ahora escribimos la lista en el archivo, linea por linea.
                    string line;
                    foreach (T item in list)
                    {
                        line = string.Empty;
                        foreach (PropertyInfo property in typeof(T).GetProperties())
                        {
                            line += string.Format("{0}{1}", property.GetValue(item), separator);
                        }
                        sw.WriteLine(line.Remove(line.Length - 1));
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
                    List<string> headers = GetHeadersFromString(sr.ReadLine(), separator);

                    String line;
                    T newObj;
                    while ((line = sr.ReadLine()) != null)
                    {
                        newObj = SerializeLineToObjectOfType<T>(headers, line, separator);
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

        private static List<string> GetHeadersFromString(string line, char separator)
        {
            List<string> headers = new List<string>();
            string[] lineSplit = line.Split(separator);

            for (int i = 0; i < lineSplit.Length; i++)
            {
                headers.Add(lineSplit[i]);
            }

            return headers;
        }

        private static T SerializeLineToObjectOfType<T>(List<string> headers, string line, char separator) where T : new()
        {
            string[] lineSplit = line.Split(separator);
            Dictionary<string, string> columns = new Dictionary<string, string>();
            T newObj;

            for (int i = 0; i < lineSplit.Length; i++)
            {
                columns.Add(headers[i], lineSplit[i]);
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

                        if (properties[i].GetCustomAttribute<HeaderName>() != null)
                        {
                            headerNameAttribute = properties[i].GetCustomAttribute<HeaderName>();
                            propertyName = headerNameAttribute.Name;
                        }

                        if (propertyName.Equals(header))
                        {
                            SetValueInProperty(columns, properties[i], headerNameAttribute, header, newObj);
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
            if (columns.ContainsKey(header))
            {
                property.SetValue(newObj, Convert.ChangeType(columns[header].Replace("$", string.Empty), property.PropertyType));
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
