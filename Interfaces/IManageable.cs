using DataManagement.Models;
using System;

namespace DataManagement.Interfaces
{
    public interface IManageable
    {
        string ForeignPrimaryKeyName { get; }
        ModelComposition ModelComposition { get; }
    }
}
