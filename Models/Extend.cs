using Newtonsoft.Json;
using OneData.Attributes;
using OneData.DAO;
using OneData.Extensions;
using OneData.Interfaces;
using OneData.Models.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OneData.Models
{
    [Serializable]
    public abstract class Extend<T> where T : IManageable, new()
    {
        [JsonIgnore]
        private static ModelComposition _composition = new ModelComposition(typeof(T));
        [UnmanagedProperty, JsonIgnore]
        public ModelComposition Composition { get { return _composition; } }

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectAllResult()
        {
            return Manager<T>.SelectAll(null);
        }

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectAll()
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
        public static List<T> SelectAll(QueryOptions queryOptions)
        {
            return Manager<T>.SelectAll(queryOptions).Data.ToList();
        }

        public static async Task<List<T>> SelectAllAsync(QueryOptions queryOptions)
        {
            Result<T> result = await Manager<T>.SelectAllAsync(queryOptions);
            return result.Data.ToList();
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
        public static async Task<T> SelectAsync(Expression<Func<T, bool>> expression)
        {
            Result<T> result = await Manager<T>.SelectAsync(expression, new QueryOptions() { MaximumResults = 1 });
            return result.Data.ToObject();
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

        public static async Task<List<T>> SelectListAsync(Expression<Func<T, bool>> expression)
        {
            Result<T> result = await Manager<T>.SelectAsync(expression, null);
            return result.Data.ToList();
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

        public static async Task<List<T>> SelectListAsync(Expression<Func<T, bool>> expression, QueryOptions queryOptions)
        {
            Result<T> result = await Manager<T>.SelectAsync(expression, queryOptions);
            return result.Data.ToList();
        }

        public static SelectStatement<T> Select(params Expression<Func<T, dynamic>>[] parameters)
        {
            return new SelectStatement<T>();
        }
    }
}