using DataManagement.Models;
using System.Reflection;

namespace DataManagement.Interfaces
{
    public interface IManageable
    {
        string ForeignPrimaryKeyName { get; }
        string TableName { get; }
        string Schema { get; }
        bool IsCacheEnabled { get; }
        long CacheExpiration { get; }
        PropertyInfo PrimaryKeyProperty { get; }
        PropertyInfo DateCreatedProperty { get; }
        PropertyInfo DateModifiedProperty { get; }
    }
}
