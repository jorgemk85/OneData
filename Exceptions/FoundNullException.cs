using System;

namespace OneData.Exceptions
{
    public class FoundNullException : Exception
    {
        public FoundNullException(string errorMessage) : base(errorMessage) { }

        public FoundNullException(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    }
}
