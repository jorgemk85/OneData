using DataManagement.Attributes;
using DataManagement.Interfaces;
using DataManagement.Models;
using System;
using System.Collections.Generic;

namespace DataManagement.Extensions
{
    public static class ManageableExtensions
    {
        public static object Include<T, TKey>(this IManageable<TKey> obj) where T : IManageable<TKey>, new() where TKey : struct
        {
            T foreignObject = (T)Activator.CreateInstance(typeof(T));
            Result result = foreignObject.SelectResult(new Parameter(obj.ForeignIdName, obj.Id));
            ICollection<T> collection = result.Data.ToList<T>();
            foreach (KeyValuePair<string, ForeignCollection> attribute in obj.ModelComposition.ForeignCollectionAttributes)
            {
                if (attribute.Value.Model.Equals(typeof(T)))
                {
                    obj.GetType().GetProperty(attribute.Key).SetValue(obj, collection);
                }
            }
            return obj;
        }
    }
}
