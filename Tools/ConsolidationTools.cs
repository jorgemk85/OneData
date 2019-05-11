using OneData.Enums;
using OneData.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Data.Common;
using System;
using OneData.DAO;

namespace OneData.Tools
{
    public class ConsolidationTools
    {
#if NETSTANDARD2_0
        private static IConfiguration _configuration;

        static ConsolidationTools()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }
#endif

        /// <summary>
        /// Valida que no exista una sola propiedad con valor nulo.
        /// </summary>
        /// <param name="obj">El objeto que sera validado.</param>
        /// <param name="throwError">Especifica si se debe de arrojar error o regresar false cuando se encuentren valores nulos.</param>
        /// <returns>Regresa True cuando el objeto tiene todas las propiedades asignadas, o error, cuando es lo contrario.</returns>
        public static bool PerformNullValidation(object obj, bool throwError)
        {
            PropertyInfo[] typeProperties = obj.GetType().GetProperties();

            foreach (PropertyInfo property in typeProperties)
            {
                if (property.GetValue(obj) == null)
                {
                    if (throwError)
                    {
                        throw new FoundNullException(string.Format("Se encontró un valor nulo en una propiedad del objeto de tipo {0} al crear una nueva instancia.", obj.GetType().ToString()));
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>. Solo se aceptan valores originados desde un objeto anonimo o predefinidos del mismo tipo enviado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="obj">El objeto a alimentarle los valores proporcionados.</param>
        /// <param name="values">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos del mismo tipo enviado.</param>
        /// <returns>Regresa el objeto ya alimentado de los valores.</returns>
        public static T SetValuesIntoObjectOfType<T>(T obj, dynamic values)
        {
            PropertyInfo[] typeProperties = typeof(T).GetProperties();
            PropertyInfo[] anonymousProperties = values.GetType().GetProperties();

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                foreach (PropertyInfo anonymousProperty in anonymousProperties)
                {
                    if (typeProperty.Name.Equals(anonymousProperty.Name))
                    {
                        if (typeProperty.CanWrite)
                        {
                            typeProperty.SetValue(obj, SimpleConverter.ConvertStringToType(anonymousProperty.GetValue(values).ToString(), typeProperty.PropertyType));
                        }
                        else
                        {
                            throw new SetAccessorNotFoundException(typeProperty.Name);
                        }

                        break;
                    }
                }
            }

            return obj;
        }

        internal static string GetInitialCatalog(string connectionToUse, bool overrideMySqlSkip = false)
        {
            if (Manager.ConnectionType == ConnectionTypes.MySQL && !overrideMySqlSkip)
            {
                return "def";
            }

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder
            {
                ConnectionString = GetValueFromConfiguration(connectionToUse, ConfigurationTypes.ConnectionString)
            };

            bool dbNameFound = false;
            dbNameFound = builder.TryGetValue("Initial Catalog", out object dbName);
            if (dbNameFound == false)
                dbNameFound = builder.TryGetValue("Database", out dbName);
            if (dbNameFound == false)
                throw new Exception("Failed to extract database name from connection string.");

            return Convert.ToString(dbName);
        }

        /// <summary>
        /// Obtiene el valor colocado bajo la llave proporcionada en el archivo de Configuracion del proyecto.
        /// </summary>
        /// <param name="key">Llave a localizar.</param>
        /// <param name="type">Especifica el tipo de configuracion al que pertenece la llave.</param>
        /// <returns>Regresa el valor obtenido del archivo de configuracion, si la llave fue encontrada.</returns>
        public static string GetValueFromConfiguration(string key, ConfigurationTypes type)
        {
#if NET461
            switch (type)
            {
                case ConfigurationTypes.ConnectionString:
                    if (ConfigurationManager.ConnectionStrings[key] == null) throw new ConfigurationNotFoundException(key);
                    return ConfigurationManager.ConnectionStrings[key].ConnectionString;
                case ConfigurationTypes.AppSetting:
                    if (ConfigurationManager.AppSettings[key] == null) throw new ConfigurationNotFoundException(key);
                    return ConfigurationManager.AppSettings[key];
                default:
                    throw new ConfigurationNotFoundException(key);
                    
            }
#endif
#if NETSTANDARD2_0
            switch (type)
            {
                case ConfigurationTypes.ConnectionString:
                    if (_configuration.GetSection("ConnectionStrings")[key] == null) throw new ConfigurationNotFoundException(key);
                    return _configuration.GetSection("ConnectionStrings")[key];
                case ConfigurationTypes.AppSetting:
                    if (_configuration.GetSection("AppSettings")[key] == null) throw new ConfigurationNotFoundException(key);
                    return _configuration.GetSection("AppSettings")[key];
                default:
                    throw new ConfigurationNotFoundException(key);
            }
#endif
        }


    }
}
