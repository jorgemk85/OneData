using System;

namespace DataManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UnmanagedProperty : Attribute
    {
        public UnmanagedProperty() { }
    }
}
