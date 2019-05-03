using System;

namespace DataManagement.Exceptions
{
    public class NotMatchingExpressionTypeException : Exception
    {
        const string errorMessage = "La expression de tipo '{0}' no puede convertirse en el tipo enviado '{1}'.";

        public NotMatchingExpressionTypeException(string originalExpressionBodyType, string requiredExpressionBodyType) : base(string.Format(errorMessage, originalExpressionBodyType, requiredExpressionBodyType)) { }

        public NotMatchingExpressionTypeException(string originalExpressionBodyType, string requiredExpressionBodyType, Exception innerException) : base(string.Format(errorMessage, originalExpressionBodyType, requiredExpressionBodyType), innerException) { }
    }
}
