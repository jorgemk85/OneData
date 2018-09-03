using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataManagement.Models
{
    [Serializable]
    public abstract class Cope<T> where T : Cope<T>, IManageable, new()
    {
        [UnmanagedProperty]
        public ref readonly string PrimaryKeyName => ref Manager<T>.ModelComposition.PrimaryPropertyName;

        [UnmanagedProperty]
        public ref readonly string DateCreatedName => ref Manager<T>.ModelComposition.DateCreatedName;

        [UnmanagedProperty]
        public ref readonly string DateModifiedName => ref Manager<T>.ModelComposition.DateModifiedName;

        [UnmanagedProperty]
        public ref readonly string TableName => ref Manager<T>.ModelComposition.TableName;

        [UnmanagedProperty]
        public ref readonly string Schema => ref Manager<T>.ModelComposition.Schema;

        [UnmanagedProperty]
        public ref readonly bool IsCacheEnabled => ref Manager<T>.ModelComposition.IsCacheEnabled;

        [UnmanagedProperty]
        public ref readonly long CacheExpiration => ref Manager<T>.ModelComposition.CacheExpiration;

        [UnmanagedProperty]
        public ref readonly string ForeignPrimaryKeyName => ref Manager<T>.ModelComposition.ForeignPrimaryKeyName;

        [UnmanagedProperty]
        public ModelComposition ModelComposition { get; } = Manager<T>.ModelComposition;

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
