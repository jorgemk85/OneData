using System;

namespace DataManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignModel : Attribute
    {
        public Type Model { get; set; }

        public ForeignModel(Type model)
        {
            Model = model;
        }
    }
}
