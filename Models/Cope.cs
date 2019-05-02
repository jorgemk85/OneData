using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataManagement.Models
{
    [Serializable]
    public abstract class Cope<T> where T : Cope<T>, IManageable, new()
    {
        [JsonIgnore]
        static readonly ModelComposition _modelComposition = new ModelComposition(typeof(T));
        [JsonIgnore]
        static readonly Configuration _configuration = new Configuration();

        [UnmanagedProperty, JsonIgnore]
        internal static ModelComposition ModelComposition { get; } = _modelComposition;
        [UnmanagedProperty, JsonIgnore]
        public Configuration Configuration { get; } = _configuration;

        static Cope()
        {
            SetupConfiguration();
        }

        private static void SetupConfiguration()
        {
            _configuration.PrimaryKeyProperty = _modelComposition.PrimaryKeyProperty;
            _configuration.DateCreatedProperty = _modelComposition.DateCreatedProperty;
            _configuration.DateModifiedProperty = _modelComposition.DateModifiedProperty;
            _configuration.CacheExpiration = _modelComposition.CacheExpiration;
            _configuration.ForeignPrimaryKeyName = _modelComposition.ForeignPrimaryKeyName;
            _configuration.IsCacheEnabled = _modelComposition.IsCacheEnabled;
            _configuration.Schema = _modelComposition.Schema;
            _configuration.TableName = _modelComposition.TableName;
        }

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectAll()
        {
            return Manager<T>.SelectAll(null);
        }

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectAllList()
        {
            return Manager<T>.SelectAll(null).Data.ToList();
        }

        public static IEnumerable<T> SelectAllIEnumerable()
        {
            return Manager<T>.SelectAll(null).Data.ToIEnumerable();
        }

        /// <summary>
        /// Obtiene un listado limitado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache. Se puede estipular un a partir de que registro se desea obtener.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectAllList(QueryOptions queryOptions)
        {
            return Manager<T>.SelectAll(queryOptions).Data.ToList();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        [Obsolete("Este metodo no debe de utilizarse. Por favor utilice el Select con expresion lambda.", true)]
        public static Result<T> Select(params Parameter[] parameters)
        {
            return null;
        }

        /// <summary>
        /// Obtiene un objeto de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static T Select(Expression<Func<T, bool>> expression)
        {
            return Manager<T>.Select(expression, new QueryOptions() { MaximumResults = 1 }).Data.ToObject();
        }

        /// <summary>
        /// Obtiene un objeto de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectResult(Expression<Func<T, bool>> expression)
        {
            return Manager<T>.Select(expression, new QueryOptions() { MaximumResults = 1 });
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectList(Expression<Func<T, bool>> expression)
        {
            return Manager<T>.Select(expression, null).Data.ToList();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectList(Expression<Func<T, bool>> expression, QueryOptions queryOptions)
        {
            return Manager<T>.Select(expression, queryOptions).Data.ToList();
        }
    }
}
