using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models
{
    [Serializable]
    public abstract class Cope<T> where T : Cope<T>, IManageable, new()
    {
        static readonly ModelComposition _modelComposition = new ModelComposition(typeof(T));
        static readonly Composition _composition = new Composition();

        [UnmanagedProperty]
        internal static ModelComposition ModelComposition { get; } = _modelComposition;
        [UnmanagedProperty]
        public Composition Composition { get; } = _composition;

        static Cope()
        {
            SetupComposition();
        }

        private static void SetupComposition()
        {
            _composition.CacheExpiration = ModelComposition.CacheExpiration;
            _composition.ForeignPrimaryKeyName = ModelComposition.ForeignPrimaryKeyName;
            _composition.IsCacheEnabled = ModelComposition.IsCacheEnabled;
            _composition.Schema = ModelComposition.Schema;
            _composition.TableName = ModelComposition.TableName;
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
    }
}
