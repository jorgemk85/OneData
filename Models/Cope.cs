using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using DataManagement.Tools;
using System;
using System.Linq.Expressions;
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
        public Composition Composition { get; } = Manager<T>.Composition;

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> SelectAll()
        {
            return Manager<T>.SelectAll();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> Select(params Parameter[] parameters)
        {
            return Manager<T>.Select(null, parameters);
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados via una expresion.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T> Select(Expression<Func<T, bool>> expression)
        {
            return Manager<T>.Select(null, ConsolidationTools.GetParametersFromExpression(expression));
        }
    }
}
