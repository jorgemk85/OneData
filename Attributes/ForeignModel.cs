using DataManagement.Enums;
using System;

namespace DataManagement.Attributes
{
    /// <summary>
    /// Atributo usado para establecer relacion entre la propiedad y el Id de una clase foranea. Ambas clases deben implementar IManageable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignModel : Attribute
    {
        /// <summary>
        /// Representa el tipo de la clase a la que se desea generar una relacion.
        /// </summary>
        public Type Model { get; set; }

        /// <summary>
        /// Especifica la accion a tomar cuando se ejecute una accion DELETE sobre la llave foranea.
        /// </summary>
        public ForeignModelActionTypes Action { get; set; }

        /// <summary>
        /// Genera una nueva instancia y recibe como parametro el tipo de la clase a relacionar.
        /// </summary>
        /// <param name="model">Representa el tipo de la clase a la que se desea generar una relacion.</param>
        /// <param name="action">Especifica la accion a tomar cuando se ejecute una accion DELETE sobre la llave foranea.</param>
        public ForeignModel(Type model, ForeignModelActionTypes action = ForeignModelActionTypes.NO_ACTION)
        {
            Model = model;
            Action = action;
        }
    }
}
