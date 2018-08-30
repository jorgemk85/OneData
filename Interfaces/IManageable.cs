using DataManagement.Models;

namespace DataManagement.Interfaces
{
    public interface IManageable<TKey> where TKey : struct
    {
        TKey? Id { get; set; }
        ModelComposition ModelComposition { get; }
        string ForeignIdName { get; }

        Result SelectResult(params Parameter[] parameters);
    }
}
