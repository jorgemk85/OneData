using System;

namespace DataManagement.Exceptions
{
    public class IManageableNotImplementedException : Exception
    {
        const string errorMessage = "El objeto no puede ser utilizado ya que este no implementa IManageable.";

        public IManageableNotImplementedException() : base(errorMessage) { }

        public IManageableNotImplementedException(Exception innerException) : base(errorMessage, innerException) { }
    }
}
