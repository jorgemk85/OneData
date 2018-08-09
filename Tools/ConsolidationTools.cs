using DataManagement.Exceptions;
using System.Reflection;

namespace DataManagement.Tools
{
    public class ConsolidationTools
    {
        /// <summary>
        /// Valida que no exista una sola propiedad con valor nulo.
        /// </summary>
        /// <param name="obj">El objeto que sera validado.</param>
        /// <returns>Regresa True cuando el objeto tiene todas las propiedades asignadas, o error, cuando es lo contrario.</returns>
        public static bool PerformNullValidation(object obj)
        {
            PropertyInfo[] typeProperties = obj.GetType().GetProperties();

            foreach (PropertyInfo property in typeProperties)
            {
                if (property.GetValue(obj) == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>. Solo se aceptan valores originados desde un objeto anonimo o predefinidos del mismo tipo enviado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="obj">El objeto a alimentarle los valores proporcionados.</param>
        /// <param name="values">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos del mismo tipo enviado.</param>
        /// <returns>Regresa el objeto ya alimentado de los valores.</returns>
        public static T SetValuesIntoObjectOfType<T>(T obj, dynamic values)
        {
            PropertyInfo[] typeProperties = typeof(T).GetProperties();
            PropertyInfo[] anonymousProperties = values.GetType().GetProperties();

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                foreach (PropertyInfo anonymousProperty in anonymousProperties)
                {
                    if (typeProperty.Name.Equals(anonymousProperty.Name))
                    {
                        if (typeProperty.CanWrite)
                        {
                            typeProperty.SetValue(obj, SimpleConverter.ConvertStringToType(anonymousProperty.GetValue(values).ToString(), typeProperty.PropertyType));
                        }
                        else
                        {
                            throw new SetAccessorNotFoundException(typeProperty.Name);
                        }

                        break;
                    }
                }
            }

            return obj;
        }
    }
}
