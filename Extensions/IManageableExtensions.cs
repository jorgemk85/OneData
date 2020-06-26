using OneData.DAO;
using OneData.Interfaces;
using OneData.Models;
using OneData.Models.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OneData.Extensions
{
    public static class IManageableExtensions
    {
        /// <summary>
        /// Borra el objeto en la base de datos segun su Id y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Delete<T>(this T obj) where T : IManageable, new()
        {
            Manager<T>.Delete(obj, null);
        }

        /// <summary>
        /// Actualiza el objeto en la base de datos y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <param name="doValidation">Indica si se desea realizar la validacion de nulos.</param>
        public static void Update<T>(this T obj, bool doValidation = false) where T : IManageable, new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Update(obj, null);
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. 
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Insert<T>(this T obj, bool doValidation = false) where T : IManageable, new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Insert(obj, null);
        }

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectAll<T>(this T obj) where T : IManageable, new()
        {
            return Manager<T>.SelectAll(null).Data.ToList();
        }


        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectAllResult<T>(this T obj) where T : IManageable, new()
        {
            return Manager<T>.SelectAll(null);
        }

        public static IEnumerable<T> SelectAllIEnumerable<T>(this T obj) where T : IManageable, new()
        {
            return Manager<T>.SelectAll(null).Data.ToIEnumerable();
        }

        /// <summary>
        /// Obtiene un listado limitado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache. Se puede estipular un a partir de que registro se desea obtener.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectAll<T>(this T obj, QueryOptions queryOptions) where T : IManageable, new()
        {
            return Manager<T>.SelectAll(queryOptions).Data.ToList();
        }

        public static async Task<List<T>> SelectAllAsync<T>(this T obj, QueryOptions queryOptions) where T : IManageable, new()
        {
            Result<T> result = await Manager<T>.SelectAllAsync(queryOptions);
            return result.Data.ToList();
        }

        /// <summary>
        /// Obtiene un objeto de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static T Select<T>(this T obj, Expression<Func<T, bool>> expression) where T : IManageable, new()
        {
            return Manager<T>.Select(expression, new QueryOptions() { MaximumResults = 1 }).Data.ToObject();
        }

        /// <summary>
        /// Obtiene un objeto de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static async Task<T> SelectAsync<T>(this T obj, Expression<Func<T, bool>> expression) where T : IManageable, new()
        {
            Result<T> result = await Manager<T>.SelectAsync(expression, new QueryOptions() { MaximumResults = 1 });
            return result.Data.ToObject();
        }

        /// <summary>
        /// Obtiene un objeto de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectResult<T>(this T obj, Expression<Func<T, bool>> expression) where T : IManageable, new()
        {
            return Manager<T>.Select(expression, new QueryOptions() { MaximumResults = 1 });
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectList<T>(this T obj, Expression<Func<T, bool>> expression) where T : IManageable, new()
        {
            return Manager<T>.Select(expression, null).Data.ToList();
        }

        public static async Task<List<T>> SelectListAsync<T>(this T obj, Expression<Func<T, bool>> expression) where T : IManageable, new()
        {
            Result<T> result = await Manager<T>.SelectAsync(expression, null);
            return result.Data.ToList();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado en forma de una lista que incluye la coleccion obtenida por la consulta.</returns>
        public static List<T> SelectList<T>(this T obj, Expression<Func<T, bool>> expression, QueryOptions queryOptions) where T : IManageable, new()
        {
            return Manager<T>.Select(expression, queryOptions).Data.ToList();
        }

        public static async Task<List<T>> SelectListAsync<T>(this T obj, Expression<Func<T, bool>> expression, QueryOptions queryOptions) where T : IManageable, new()
        {
            Result<T> result = await Manager<T>.SelectAsync(expression, queryOptions);
            return result.Data.ToList();
        }

        public static SelectStatement<T> Select<T>(this T obj, params Expression<Func<T, dynamic>>[] parameters) where T : IManageable, new()
        {
            return new SelectStatement<T>();
        }
    }
}
