using FastMember;
using OneData.Attributes;
using System;

namespace OneData.Models
{
    internal sealed class OneProperty
    {
        public string Name { get; set; }
        public string PropertyName { get; set; }
        public TypeAccessor Accesor { get; set; }    
        public Type PropertyType { get; set; }
        public Type ReflectedType { get; set; }
        public AllowNull AllowNullAttribute { get; set; }
        public Default DefaultAttribute { get; set; }
        public PrimaryKey PrimaryKeyAttribute { get; set; }
        public DateCreated DateCreatedAttibute { get; set; }
        public DateModified DateModifiedAttribute { get; set; }
        public Unique UniqueAttribute { get; set; }
        public AutoProperty AutoPropertyAttribute { get; set; }
        public DataLength DataLengthAttribute { get; set; }
        public ForeignData ForeignDataAttribute { get; set; }
        public ForeignKey ForeignKeyAttribute { get; set; }
        public UnmanagedProperty UnmanagedPropertyAttribute { get; set; }

        internal object GetValue(object model)
        {
            return Accesor[model, PropertyName];
        }

        internal void SetValue(object model, object value)
        {
            Accesor[model, PropertyName] = value;
        }
    }
}
