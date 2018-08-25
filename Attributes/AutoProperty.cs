using DataManagement.Standard.Enums;
using System;

namespace DataManagement.Standard.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoProperty : Attribute
    {
        public AutoPropertyTypes AutoPropertyType { get; set; }

        public AutoProperty(AutoPropertyTypes type)
        {
            AutoPropertyType = type;
        }
    }
}
