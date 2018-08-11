using DataManagement.DAO;
using DataManagement.Interfaces;

namespace DataManagement.Extensions
{
    public static class IManageableExtensions
    {
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
        public static void Update<T>(this IManageable obj, bool doValidation = false) where T : new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Update((T)obj, true);
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. Esta funcion realiza la validacion de nulos por default.
        /// </summary>
        public static void Insert<T>(this IManageable obj) where T : new()
        {
            obj.Validate();
            Manager<T>.Insert((T)obj, true);
        }
    }
}
