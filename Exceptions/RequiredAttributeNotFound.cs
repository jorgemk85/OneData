using System;

namespace DataManagement.Exceptions
{
    public class RequiredAttributeNotFound : Exception
    {
        const string errorMessage = "No se encontro el atributo '{0}' el cual, es requerido en la clase '{1}'.";

        public RequiredAttributeNotFound(string attributeName, string className) : base(string.Format(errorMessage, attributeName, className)) { }

        public RequiredAttributeNotFound(string attributeName, string className, Exception innerException) : base(string.Format(errorMessage, attributeName, className), innerException) { }
    }
}
