using System;

namespace DataManagement.Exceptions
{
    public class ConvertionFailedException : Exception
    {
        const string errorMessage = "No se pudo convertir el valor '{0}' al tipo '{1}'.";

        public ConvertionFailedException(string value, Type targetType) : base(string.Format(errorMessage, value, targetType.ToString())) { }

        public ConvertionFailedException(string value, Type targetType, Exception innerException) : base(string.Format(errorMessage, value, targetType.ToString()), innerException) { }
    }
}
