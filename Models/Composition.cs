using System.Reflection;

namespace DataManagement.Models
{
    public sealed class Composition
    {
        public string TableName { get; internal set; }
        public string Schema { get; internal set; }
        public bool IsCacheEnabled { get; internal set; }
        public long CacheExpiration { get; internal set; }
        public string ForeignPrimaryKeyName { get; internal set; }
    }
}
