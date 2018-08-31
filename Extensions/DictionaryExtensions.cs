using DataManagement.Tools;
using System.Collections.Generic;

namespace DataManagement.Extensions
{
    public static class DictionaryExtensions
    {
        public static T ToObject<TKey, T>(this Dictionary<TKey, T> dictionary) where T : new()
        {
            return DataSerializer.ConvertDictionaryToObjectOfType(dictionary);
        }

        public static List<T> ToList<TKey, T>(this Dictionary<TKey, T> dictionary) where T : new()
        {
            return DataSerializer.ConvertDictionaryToListOfType(dictionary);
        }
    }
}
