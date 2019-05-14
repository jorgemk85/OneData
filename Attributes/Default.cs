using System;

namespace OneData.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Default : Attribute
    {
        public string Value { get; set; }

        public Default(string value)
        {
            Value = value;
        }
    }
}
