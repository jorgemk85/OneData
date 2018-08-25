using DataManagement.Standard.Attributes;
using DataManagement.Standard.Enums;
using System;

namespace DataManagement.Standard.Interfaces
{
    public interface IManageable<TKey> where TKey : struct
    {
        #region Primary Property
        [PrimaryProperty]
        TKey? Id { get; set; }
        #endregion

        #region Auto Properties
        [DateCreatedProperty, AutoProperty(AutoPropertyTypes.DateTime)]
        DateTime? FechaCreacion { get; set; }
        [DateModifiedProperty, AutoProperty(AutoPropertyTypes.DateTime)]
        DateTime? FechaModificacion { get; set; }
        #endregion

        #region Unmanaged Properties
        /// <summary>
        /// Almacena el nombre de la tabla en la base de datos.
        /// </summary>
        [UnmanagedProperty]
        string DataBaseTableName { get; }
        /// <summary>
        /// Almacena el nombre del schema de la tabla en la base de datos.
        /// </summary>
        [UnmanagedProperty]
        string Schema { get; }
        /// <summary>
        /// Especifica si se desea utilizar las funciones de cache en la clase actual.
        /// </summary>
        [UnmanagedProperty]
        bool IsCacheEnabled { get; }
        /// <summary>
        /// Si se estan utilizando las funciones de cache, se puede especificar la expiracion o vigencia del mismo en segundos.
        /// </summary>
        [UnmanagedProperty]
        int CacheExpiration { get; }
        #endregion
    }
}
