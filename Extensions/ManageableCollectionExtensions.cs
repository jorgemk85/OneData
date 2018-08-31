using DataManagement.Attributes;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataManagement.Extensions
{
    public static class ManageableCollectionExtensions
    {
        public static ManageableCollection<TKey, TProperty> Include<TKey, TProperty>(this ManageableCollection<TKey, TProperty> collection, Type target) where TProperty : Cope<TProperty, TKey>, new() where TKey : struct
        {
            var foreignObject = Activator.CreateInstance(target);
            MethodInfo method = target.GetMethod(nameof(Cope<TProperty, TKey>.SelectResult), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (KeyValuePair<TKey, TProperty> item in collection)
            {
                var result = method.Invoke(foreignObject, new[] { new Parameter[] { new Parameter(item.Value.ForeignIdName, item.Value.Id) } });

                foreach (KeyValuePair<string, ForeignCollection> attribute in item.Value.ModelComposition.ForeignCollectionAttributes)
                {
                    if (attribute.Value.Model.Equals(target))
                    {
                        typeof(TProperty).GetProperty(attribute.Key).SetValue(item.Value, result.GetType().GetProperty(nameof(Result<TProperty, TKey>.Collection)).GetValue(result));
                    }
                }
            }

            return collection;
        }

        public static T ToObject<TKey, T>(this ManageableCollection<TKey, T> dictionary) where T : new()
        {
            return DataSerializer.ConvertManageableCollectionToObjectOfType(dictionary);
        }

        public static List<T> ToList<TKey, T>(this ManageableCollection<TKey, T> dictionary) where T : new()
        {
            return DataSerializer.ConvertManageableCollectionToListOfType(dictionary);
        }
    }
}
