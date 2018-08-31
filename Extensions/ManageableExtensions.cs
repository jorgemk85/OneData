using DataManagement.Models;
using System;

namespace DataManagement.Extensions
{
    public static class ManageableExtensions
    {
        public static T Include<T, TKey>(this Cope<T, TKey> obj, Type target) where T : Cope<T, TKey>, new() where TKey : struct
        {
            return obj.Include(obj, target);
        }
    }
}
