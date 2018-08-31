using DataManagement.Attributes;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataManagement.Extensions
{
    public static class CopeExtensions
    {
        public static T Include<T, TKey>(this Cope<T, TKey> obj, Type target) where T : Cope<T, TKey>, new() where TKey : struct
        {
            var foreignObject = Activator.CreateInstance(target);
            MethodInfo method = target.GetMethod(nameof(Cope<T, TKey>.SelectResult),  BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var result = method.Invoke(foreignObject, new[] { new Parameter[] { new Parameter(obj.ForeignIdName, obj.Id) } });

            foreach (KeyValuePair<string, ForeignCollection> attribute in obj.ModelComposition.ForeignCollectionAttributes)
            {
                if (attribute.Value.Model.Equals(target))
                {
                    typeof(T).GetProperty(attribute.Key).SetValue(obj, result.GetType().GetProperty(nameof(Result<T,TKey>.Collection)).GetValue(result));
                }
            }
            return (T)obj;
        }

    }
}
