using DataManagement.Exceptions;
using DataManagement.Tools;
using System.Reflection;

namespace DataManagement.Extensions
{
    public static class ObjectExtensions
    {
        public static T New<T>(this T obj, dynamic parameters) where T : new()
        {
            PropertyInfo[] typeProperties = typeof(T).GetProperties();
            PropertyInfo[] anonymousProperties = parameters.GetType().GetProperties();

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                object value = typeProperty.GetValue(obj);

                foreach (PropertyInfo anonymousProperty in anonymousProperties)
                {
                    if (typeProperty.Name.Equals(anonymousProperty.Name))
                    {
                        if (typeProperty.CanWrite)
                        {
                            typeProperty.SetValue(obj, SimpleConverter.ConvertStringToType(anonymousProperty.GetValue(parameters).ToString(), typeProperty.PropertyType));
                        }

                        break;
                    }
                }
            }

            return obj;
        }

        public static bool Validate(this object obj)
        {
            return PerformNullValidation(obj);
        }

        private static bool PerformNullValidation(object obj)
        {
            PropertyInfo[] typeProperties = obj.GetType().GetProperties();

            string nullProperties = string.Empty;
            for (int i = 0; i < typeProperties.Length; i++)
            {
                if (typeProperties[i].GetValue(obj) == null)
                {
                    nullProperties += string.Format("{0},", typeProperties[i].Name);
                }
            }

            if (!string.IsNullOrWhiteSpace(nullProperties))
            {
                throw new FoundNullsException(nullProperties.Substring(0, nullProperties.Length - 1));
            }

            return true;
        }
    }
}
