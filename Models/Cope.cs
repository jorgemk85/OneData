using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Enums;
using System;

namespace DataManagement.Models
{
    /// <summary>
    /// Clase principal de la que tienen que heredar todos los objetos de negocio que se desee utilizar con la libreria DataManagement.
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
        public ref ModelComposition ModelComposition => ref Manager<T, TKey>.ModelComposition;
        #endregion

        #region Constructors
        // TODO: Necesitamos encontrar una manera mas facil de instanciar objetos con nuevos Ids irrepetibles. 
        public Cope(TKey id)
        {
            Id = id;
        }
        #endregion
    }
}
