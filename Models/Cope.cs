using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataManagement.Models
{
    [Serializable]
    public abstract class Cope<T> where T : Cope<T>, IManageable, new()
    {
        [UnmanagedProperty]
        internal PropertyInfo PrimaryKeyProperty { get; } = Manager<T>.ModelComposition.PrimaryKeyProperty;
        [UnmanagedProperty]
        internal PropertyInfo DateCreatedProperty { get; } = Manager<T>.ModelComposition.DateCreatedProperty;
        [UnmanagedProperty]
        internal PropertyInfo DateModifiedProperty { get; } = Manager<T>.ModelComposition.DateModifiedProperty;
        [UnmanagedProperty]
        public string TableName { get; } = Manager<T>.ModelComposition.TableName;
        [UnmanagedProperty]
        public string Schema { get; } = Manager<T>.ModelComposition.Schema;
        [UnmanagedProperty]
        public bool IsCacheEnabled { get; } = Manager<T>.ModelComposition.IsCacheEnabled;
        [UnmanagedProperty]
        public long CacheExpiration { get; } = Manager<T>.ModelComposition.CacheExpiration;
        [UnmanagedProperty]
        public string ForeignPrimaryKeyName { get; } = Manager<T>.ModelComposition.ForeignPrimaryKeyName;

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa la coleccion obtenida ya convertida en una lista del tipo <typeparamref name="T"/></returns>
        public static List<T> SelectAll()
        {
            return Manager<T>.SelectAll().Data.ToList();
        }

        /// <summary>
        /// Obtiene el primer objeto de tipo <typeparamref name="T"/> almacenado en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static T Select(params Parameter[] parameters)
        {
            return Manager<T>.Select(null, parameters).Data.ToObject();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa la coleccion obtenida ya convertida en una lista del tipo <typeparamref name="T"/></returns>
        public static List<T> SelectList(params Parameter[] parameters)
        {
            return Manager<T>.Select(null, parameters).Data.ToList();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectResult(params Parameter[] parameters)
        {
            return Manager<T>.Select(null, parameters);
        }
    }
}
