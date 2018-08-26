using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Exceptions
{
    /// <summary>
    /// Ocurre cuando no se enviaron parametros al cache para el tipo de consulta Select. 
    /// </summary>
    public class InvalidNumberOfParametersException : Exception
    {
        const string errorMessage = "Al parecer no se establecieron paramentros para la busqueda o no se obtuvieron resultados previos a la consulta. Por favor asegurece de por lo menos colocar uno valido y volver a intentar.";

        public InvalidNumberOfParametersException() : base(errorMessage) { }

        public InvalidNumberOfParametersException(Exception innerException) : base(errorMessage, innerException) { }
    }
}
