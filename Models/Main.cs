using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models
{
    /// <summary>
    /// Clase principal de la que tienen que heredar todos los objetos de negocio que se desee utilizar con la libreria DataManagement.
    /// </summary>
    [Serializable]
    public abstract class Main : IManageable
    {
        #region Properties
        public Guid? Id { get; set; }
        #endregion

        #region Unlinked Properties
        [UnlinkedProperty]
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;
        [UnlinkedProperty]
        public DateTime? FechaModificacion { get; set; } = DateTime.Now;
        /// <summary>
        /// Almacena el nombre de la tabla en la base de datos SIN prefijos ni sufijos.
        /// </summary>
        [UnlinkedProperty]
        public string DataBaseTableName { get; }
        /// <summary>
        /// Almacena el nombre del schema de la tabla en la base de datos.
        /// </summary>
        [UnlinkedProperty]
        public string Schema { get; }
        /// <summary>
        /// Especifica si se desea utilizar las funciones de cache en la clase actual.
        /// </summary>
        [UnlinkedProperty]
        public bool IsCacheEnabled { get; }
        /// <summary>
        /// Si se estan utilizando las funciones de cache, se puede especificar la expiracion o vigencia del mismo en segundos.
        /// </summary>
        [UnlinkedProperty]
        public int CacheExpiration { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Construye el objeto con las propiedades Schema, IsCacheEnabled y CacheExpiration predefinidas con valores predeterminados.
        /// </summary>
        /// <param name="id">El identificador unico del objeto.</param>
        /// <param name="dbTableName">Nombre de la tabla en la base de datos.</param>
        public Main(Guid id, string dbTableName)
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
        public Main(Guid id, string dbTableName, string schema)
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
        public Main(Guid id, string dbTableName, bool isCacheEnabled, int cacheExpiration)
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
        public Main(Guid id, string dbTableName, string schema, bool isCacheEnabled, int cacheExpiration)
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
