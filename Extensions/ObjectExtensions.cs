using DataManagement.Exceptions;
using DataManagement.Tools;
using System.Reflection;

namespace DataManagement.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="parameters">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos.</param>
        public static void New<T>(this T obj, dynamic parameters) where T : new()
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
        }

        /// <summary>
        /// Valida que no exista una sola propiedad con valor nulo.
        /// </summary>
        /// <returns>Regresa True cuando el objeto tiene todas las propiedades asignadas, o error, cuando es lo contrario.</returns>
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
