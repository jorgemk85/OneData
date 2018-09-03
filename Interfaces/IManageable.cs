using DataManagement.Models;

namespace DataManagement.Interfaces
{
    public interface IManageable
    {
        string PrimaryKeyName { get; }
        string ForeignPrimaryKeyName { get; }
        string DateCreatedName { get; }
        string DateModifiedName { get; }
        string TableName { get; }
        string Schema { get; }
        bool IsCacheEnabled { get; }
        long CacheExpiration { get; }
    }
}
