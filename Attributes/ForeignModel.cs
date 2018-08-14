using DataManagement.Interfaces;
using System;

namespace DataManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignModel : Attribute
    {
        public Type Model { get; set; }

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
