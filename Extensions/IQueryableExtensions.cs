using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneData.Extensions
{
    public static class IQueryableExtensions
    {
        public static Dictionary<dynamic, T> ToDictionary<T>(this IQueryable<T> queryable, string keyName, Type keyType) where T : IManageable, new()
        {
            return DataSerializer.ConvertQueryableToDictionaryOfType<T>(queryable, keyName, keyType);
        }
    }
}
