using System;

namespace DataManagement.Standard.Exceptions
{
    public class SetAccessorNotFoundException : Exception
    {
        public SetAccessorNotFoundException(string propertyName) : base(string.Format("La propiedad {0} no contiene el accesor Set para asignarle un valor.", propertyName)) { }

        public SetAccessorNotFoundException(string propertyName, Exception innerException) : base(string.Format("La propiedad {0} no contiene el accesor Set para asignarle un valor.", propertyName), innerException) { }
    }
}
