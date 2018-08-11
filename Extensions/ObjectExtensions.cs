using DataManagement.DAO;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Tools;
using System;

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
        public static T Fill<T>(this T obj, dynamic values)
        {
            return ConsolidationTools.SetValuesIntoObjectOfType(obj, values);
        }

        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>. Solo se aceptan valores originados desde un objeto anonimo o predefinidos del mismo tipo enviado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="values">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos del mismo tipo enviado.</param>
        /// <returns>Regresa el objeto ya alimentado de los valores.</returns>
        public static bool FillAndValidate<T>(this T obj, dynamic values)
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
        public static T Validate<T>(this T obj)
        {
            ConsolidationTools.PerformNullValidation(obj, true);

            return obj;
        }

        /// <summary>
        /// Borra el objeto en la base de datos segun su Id y en su supuesto, tambien en el cache.
        /// </summary>
        public static void Delete<T>(this T obj) where T : new()
        {
            Manager<T>.Delete(obj, true);
        }

        /// <summary>
        /// Actualiza el objeto en la base de datos y en su supuesto, tambien en el cache.
        /// </summary>
        /// <param name="doValidation">Indica si se desea realizar la validacion de nulos.</param>
        /// <returns></returns>
        public static void Update<T>(this T obj, bool doValidation = false) where T : new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Update(obj, true);
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. Esta funcion realiza la validacion de nulos por default.
        /// </summary>
        public static void Insert<T>(this T obj) where T : new()
        {
            obj.Validate();
            Manager<T>.Insert(obj, true);
        }
    }
}
