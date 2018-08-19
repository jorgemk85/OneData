using System;

namespace DataManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class InternalProperty : Attribute
    {
        public InternalProperty() { }
    }
}
