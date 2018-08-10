using System;

namespace DataManagement.Exceptions
{
    public class FoundNullException : Exception
    {
        public FoundNullException(string errorMessage) : base(errorMessage) { }

        public FoundNullException(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    }
}
