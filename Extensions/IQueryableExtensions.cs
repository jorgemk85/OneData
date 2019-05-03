using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Extensions
{
    public static class IQueryableExtensions
    {
        public static Dictionary<dynamic, T> ToDictionary<T>(this IQueryable<T> queryable, string keyName, Type keyType) where T : Cope<T>, IManageable, new()
        {
            return DataSerializer.ConvertQueryableToDictionaryOfType<T>(queryable, keyName, keyType);
        }
    }
}
