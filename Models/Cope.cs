using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using DataManagement.Tools;
using System;
using System.Linq.Expressions;

namespace DataManagement.Models
{
    [Serializable]
    public abstract class Cope<T> where T : Cope<T>, IManageable, new()
    {
        static readonly ModelComposition _modelComposition = new ModelComposition(typeof(T));
        static readonly Configuration _configuration = new Configuration();

        [UnmanagedProperty]
        internal static ModelComposition ModelComposition { get; } = _modelComposition;
        [UnmanagedProperty]
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
            return Manager<T>.Select(null, ExpressionTools.ConvertExpressionToParameters(expression));
        }
    }
}
