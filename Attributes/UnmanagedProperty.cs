using System;

namespace DataManagement.Standard.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UnmanagedProperty : Attribute
    {
        public UnmanagedProperty() { }
    }
}
