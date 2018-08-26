using DataManagement.Standard.Attributes;
using DataManagement.Standard.DAO;
using DataManagement.Standard.Enums;
using System;

namespace DataManagement.Standard.Models
{
    /// <summary>
    /// Clase principal de la que tienen que heredar todos los objetos de negocio que se desee utilizar con la libreria DataManagement.Standard.
    /// </summary>
    /// <typeparam name="T">Representa el tipo de la clase que esta heredando de Cope<T, TKey>.</typeparam>
    /// <typeparam name="TKey">Representa el tipo a utilizar para la llave primaria del Id.</typeparam>
    [Serializable]
    public abstract class Cope<T, TKey> where T : Cope<T, TKey>, new() where TKey : struct
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
        [UnmanagedProperty]
        public ModelComposition ModelComposition { get; } = Manager<T, TKey>.ModelComposition;
        #endregion

        #region Constructors
        public Cope(TKey id)
        {
            Id = id;
        }
        #endregion
    }
}
