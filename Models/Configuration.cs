using OneData.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace OneData.Models
{
    public sealed class Configuration
    {
        public PropertyInfo PrimaryKeyProperty { get; internal set; }
        public PropertyInfo DateCreatedProperty { get; internal set; }
        public PropertyInfo DateModifiedProperty { get; internal set; }
        public string TableName { get; internal set; }
        public string Schema { get; internal set; }
        public bool IsCacheEnabled { get; internal set; }
        public long CacheExpiration { get; internal set; }
        public string FullyQualifiedTableName { get; set; }
        internal Dictionary<string, PropertyInfo> ManagedProperties { get; set; }
        internal Dictionary<string, PropertyInfo> UniqueKeyProperties { get; set; }
        internal Dictionary<string, PropertyInfo> DefaultProperties { get; set; }
        internal Dictionary<string, PropertyInfo> ForeignKeyProperties { get; set; }
        internal Dictionary<string, DataLength> DataLengthAttributes { get; set; }
        internal Dictionary<string, ForeignKey> ForeignKeyAttributes { get; set; }
        internal Dictionary<string, Default> DefaultAttributes { get; set; }
    }
}
