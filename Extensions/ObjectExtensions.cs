using DataManagement.Tools;

namespace DataManagement.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>. Solo se aceptan valores originados desde un objeto anonimo o predefinidos del mismo tipo enviado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="values">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos del mismo tipo enviado.</param>
        /// <returns>Regresa el objeto ya alimentado de los valores.</returns>
        public static T Fill<T, TKey>(this T obj, dynamic values)
        {
            return ConsolidationTools.SetValuesIntoObjectOfType(obj, values);
        }

        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>. Solo se aceptan valores originados desde un objeto anonimo o predefinidos del mismo tipo enviado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="values">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos del mismo tipo enviado.</param>
        /// <returns>Regresa el objeto ya alimentado de los valores.</returns>
        public static bool FillAndValidate<T, TKey>(this T obj, dynamic values)
        {
            return Validate(ConsolidationTools.SetValuesIntoObjectOfType(obj, values), true);
        }

        /// <summary>
        /// Valida que no exista una sola propiedad con valor nulo.
        /// </summary>
        /// <returns>Regresa True cuando el objeto tiene todas las propiedades asignadas.</returns>
        public static bool Validate(this object obj, bool throwError = true)
        {
            return ConsolidationTools.PerformNullValidation(obj, throwError);
        }

        /// <summary>
        /// Valida que no exista una sola propiedad con valor nulo.
        /// </summary>
        /// <returns>Regresa el objeto que fue validado.</returns>
        public static T Validate<T, TKey>(this T obj)
        {
            ConsolidationTools.PerformNullValidation(obj, true);

            return obj;
        }
    }
}
