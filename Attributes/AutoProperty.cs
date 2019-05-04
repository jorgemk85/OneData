using OneData.Enums;
using System;

namespace OneData.Attributes
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
