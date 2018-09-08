using System.Reflection;

namespace DataManagement.Models
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
        public string ForeignPrimaryKeyName { get; internal set; }
    }
}
