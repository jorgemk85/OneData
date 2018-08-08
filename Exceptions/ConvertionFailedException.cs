using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Exceptions
{
    public class ConvertionFailedException : Exception
    {
        public ConvertionFailedException(string value, Type targetType) : base(string.Format("No se pudo convertir el valor '{0}' al tipo {1}.", value, targetType.ToString())) { }

        public ConvertionFailedException(string value, Type targetType, Exception innerException) : base(string.Format("No se pudo convertir el valor '{0}' al tipo {1}.", value, targetType.ToString()), innerException) { }
    }
}
