using System;

namespace DataManagement.Exceptions
{
    public class AllowUserVariableNotEnabledException : Exception
    {
        const string errorMessage = "La cadena de conexion usada no solicita autorizacion para usar variables de usuario, las cuales son necesarias para crear tablas y procedimientos almacenados en tiempo de ejecucion. Intenta agregando 'Allow User Variables=True' a la cadena.";

        public AllowUserVariableNotEnabledException() : base(errorMessage) { }

        public AllowUserVariableNotEnabledException(Exception innerException) : base(errorMessage, innerException) { }
    }
}
