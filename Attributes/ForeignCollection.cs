using System;

namespace OneData.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignCollection : Attribute
    {
        public Type Model { get; set; }

        public ForeignCollection(Type model)
        {
            Model = model;
        }
    }
}
