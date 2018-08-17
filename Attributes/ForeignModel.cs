using DataManagement.Interfaces;
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
        /// Genera una nueva instancia y recibe como parametro el tipo de la clase a relacionar.
        /// </summary>
        /// <param name="model">Representa el tipo de la clase a la que se desea generar una relacion.</param>
        public ForeignModel(Type model)
        {
            if (model.IsAssignableFrom(typeof(IManageable)))
            {
                Model = model;
            }
            else
            {
                throw new NotSupportedException(string.Format("El tipo '{0}' no es aceptable debido a que no implementa la interfaz '{1}'.", nameof(model), nameof(IManageable)));
            }
        }
    }
}
