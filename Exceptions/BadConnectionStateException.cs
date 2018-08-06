using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Exceptions
{
    /// <summary>
    /// Ocurre cuando la conexion a la base de datos no esta abierta.
    /// </summary>
    public class BadConnectionStateException : Exception
    {
        const string errorMessage = "La conexion a la base de datos no se encuentra abierta.";

        public BadConnectionStateException() : base(errorMessage) { }

        public BadConnectionStateException(Exception innerException) : base(errorMessage, innerException) { }
    }
}
