using System;

namespace DataManagement.Exceptions
{
    public class FoundNullsException : Exception
    {
        public FoundNullsException(string nullProperties) : base(string.Format("No se asignaron todas las propiedades al objeto. Propiedades en nulo: {0}", nullProperties)) { }

        public FoundNullsException(string nullProperties, Exception innerException) : base(string.Format("No se asignaron todas las propiedades al objeto. Propiedades en nulo: {0}", nullProperties), innerException) { }
    }
}
