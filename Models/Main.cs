using DataManagement.Standard.Attributes;
using DataManagement.Standard.DAO;
using DataManagement.Standard.Enums;
using DataManagement.Standard.Interfaces;
using System;

namespace DataManagement.Standard.Models
{
    /// <summary>
    /// Clase principal de la que tienen que heredar todos los objetos de negocio que se desee utilizar con la libreria DataManagement.Standard.
    /// </summary>
    /// <typeparam name="TKey">Representa el tipo a utilizar para la llave primaria del Id.</typeparam>
    [Serializable]
    public abstract class Main<TKey> : IManageable<TKey> where TKey : struct
    {
        #region Primary Property
        [PrimaryProperty]
        public TKey? Id { get; set; }
        #endregion

        #region Auto Properties
        [DateCreatedProperty, AutoProperty(AutoPropertyTypes.DateTime)]
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;
        [DateModifiedProperty, AutoProperty(AutoPropertyTypes.DateTime)]
        public DateTime? FechaModificacion { get; set; } = DateTime.Now;
        #endregion

        #region Unmanaged Properties
        /// <summary>
        /// Almacena el nombre de la tabla en la base de datos SIN prefijos ni sufijos.
        /// </summary>
        [UnmanagedProperty]
        public string DataBaseTableName { get; }
        /// <summary>
        /// Almacena el nombre del schema de la tabla en la base de datos.
        /// </summary>
        [UnmanagedProperty]
        public string Schema { get; }
        /// <summary>
        /// Especifica si se desea utilizar las funciones de cache en la clase actual.
        /// </summary>
        [UnmanagedProperty]
        public bool IsCacheEnabled { get; }
        /// <summary>
        /// Si se estan utilizando las funciones de cache, se puede especificar la expiracion o vigencia del mismo en segundos.
        /// </summary>
        [UnmanagedProperty]
        public int CacheExpiration { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Construye el objeto con las propiedades Schema, IsCacheEnabled y CacheExpiration predefinidas con valores predeterminados.
        /// </summary>
        /// <param name="id">El identificador unico del objeto.</param>
        /// <param name="dbTableName">Nombre de la tabla en la base de datos.</param>
        public Main(TKey id, string dbTableName)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = Manager.DefaultSchema;
            IsCacheEnabled = false;
            CacheExpiration = 0;
        }

        /// <summary>
        /// Construye el objeto con las propiedades IsCacheEnabled y CacheExpiration predefinidas con valores predeterminados.
        /// </summary>
        /// <param name="id">El identificador unico del objeto.</param>
        /// <param name="dbTableName">Nombre de la tabla en la base de datos.</param>
        /// <param name="schema">Nombre del esquema de la tabla en la base de datos.</param>
        public Main(TKey id, string dbTableName, string schema)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
            IsCacheEnabled = false;
            CacheExpiration = 0;
        }

        /// <summary>
        /// Construye el objeto con la propiedad Schema predefinida con valor predeterminado.
        /// </summary>
        /// <param name="id">El identificador unico del objeto.</param>
        /// <param name="dbTableName">Nombre de la tabla en la base de datos.</param>
        /// <param name="isCacheEnabled">Indica si se desea utilizar las funciones de cache en esta clase.</param>
        /// <param name="cacheExpiration">Se especifica la expiracion o vigencia del cache en segundos.</param>
        public Main(TKey id, string dbTableName, bool isCacheEnabled, int cacheExpiration)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = Manager.DefaultSchema;
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration;
        }

        /// <summary>
        /// Construye el objeto sin valores predefinidos.
        /// </summary>
        /// <param name="id">El identificador unico del objeto.</param>
        /// <param name="dbTableName">Nombre de la tabla en la base de datos.</param>
        /// <param name="schema">Nombre del esquema de la tabla en la base de datos.</param>
        /// <param name="isCacheEnabled">Indica si se desea utilizar las funciones de cache en esta clase.</param>
        /// <param name="cacheExpiration">Se especifica la expiracion o vigencia del cache en segundos.</param>
        public Main(TKey id, string dbTableName, string schema, bool isCacheEnabled, int cacheExpiration)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration;
        }
        #endregion
    }
}
