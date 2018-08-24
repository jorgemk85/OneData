using System;

namespace DataManagement.Standard.Exceptions
{
    public class ConnectionVariableNotEnabledException : Exception
    {
        const string errorMessage = "La cadena de conexion usada no contiene la configuracion '{0}', la cual es necesaria para la correcta operacion de la libreria.";

        public ConnectionVariableNotEnabledException(string setting) : base(string.Format(errorMessage, setting)) { }

        public ConnectionVariableNotEnabledException(string setting, Exception innerException) : base(string.Format(errorMessage, setting), innerException) { }
    }
}
