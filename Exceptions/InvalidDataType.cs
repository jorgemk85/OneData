using System;

namespace DataManagement.Exceptions
{
    public class InvalidDataType : Exception
    {
        const string errorMessage = "La propiedad '{0}' dentro del tipo '{1}' debe estar declarada con el tipo de dato '{2}'.";

        public InvalidDataType(string propertyName, string className, string requiredDataType) : base(string.Format(errorMessage, propertyName, className, requiredDataType)) { }

        public InvalidDataType(string propertyName, string className, string requiredDataType, Exception innerException) : base(string.Format(errorMessage, propertyName, className, requiredDataType), innerException) { }
    }
}
