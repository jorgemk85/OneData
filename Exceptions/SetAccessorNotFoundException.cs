using System;

namespace DataManagement.Exceptions
{
    public class SetAccessorNotFoundException : Exception
    {
        const string errorMessage = "La propiedad '{0}' no contiene el accesor Set para asignarle un valor.";

        public SetAccessorNotFoundException(string propertyName) : base(string.Format(errorMessage, propertyName)) { }

        public SetAccessorNotFoundException(string propertyName, Exception innerException) : base(string.Format(errorMessage, propertyName), innerException) { }
    }
}
