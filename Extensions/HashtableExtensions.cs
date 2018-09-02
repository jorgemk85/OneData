using DataManagement.Tools;
using System.Collections;
using System.Collections.Generic;

namespace DataManagement.Extensions
{
    public static class HashtableExtensions
    {
        public static T ToObject<T>(this Hashtable hashtable) where T : new()
        {
            return DataSerializer.ConvertHashtableToObjectOfType<T>(hashtable);
        }

        public static List<T> ToList<T>(this Hashtable hashtable) where T : new()
        {
            return DataSerializer.ConvertHashtableToListOfType<T>(hashtable);
        }
    }
}
