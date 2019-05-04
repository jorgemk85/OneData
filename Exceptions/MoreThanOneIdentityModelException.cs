using System;

namespace OneData.Exceptions
{
    public class MoreThanOneIdentityModelException : Exception
    {
        const string errorMessage = "Se tiene marcada por lo menos dos clases con el atributo IdentityModel. Solo se permite su uso en una sola clase.";

        public MoreThanOneIdentityModelException() : base(errorMessage) { }

        public MoreThanOneIdentityModelException(Exception innerException) : base(errorMessage, innerException) { }
    }
}
