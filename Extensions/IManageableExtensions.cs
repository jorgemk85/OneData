using DataManagement.Standard.DAO;
using DataManagement.Standard.Interfaces;

namespace DataManagement.Standard.Extensions
{
    public static class IManageableExtensions
    {
        /// <summary>
        /// Borra el objeto en la base de datos segun su Id y en su supuesto, tambien en el cache.
        /// </summary>
        public static void Delete<T>(this T obj) where T : IManageable, new()
        {
            Manager<T>.Delete(obj);
        }

        /// <summary>
        /// Actualiza el objeto en la base de datos y en su supuesto, tambien en el cache.
        /// </summary>
        /// <param name="doValidation">Indica si se desea realizar la validacion de nulos.</param>
        /// <returns></returns>
        public static void Update<T>(this T obj, bool doValidation = false) where T : IManageable, new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Update(obj);
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. Esta funcion realiza la validacion de nulos por default.
        /// </summary>
        public static void Insert<T>(this T obj) where T : IManageable, new()
        {
            obj.Validate();
            Manager<T>.Insert(obj);
        }
    }
}
