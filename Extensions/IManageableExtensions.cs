using DataManagement.DAO;
using DataManagement.Enums;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataManagement.Extensions
{
    public static class IManageableExtensions
    {
        /// <summary>
        /// Borra el objeto en la base de datos segun su Id y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Delete<T>(this T obj) where T : Cope<T>, IManageable, new()
        {
            Manager<T>.Delete(obj, null);
        }

        /// <summary>
        /// Actualiza el objeto en la base de datos y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <param name="doValidation">Indica si se desea realizar la validacion de nulos.</param>
        public static void Update<T>(this T obj, bool doValidation = false) where T : Cope<T>, IManageable, new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Update(obj, null);
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. 
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Insert<T>(this T obj, bool doValidation = false) where T : Cope<T>, IManageable, new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Insert(obj, null);
        }

        /// <summary>
        /// Inserta una coleccion de objetos de forma masiva y en una sola llamada en la base de datos. Aun no integrado al pipeline regular de la aplicacion. ACTUALMENTE EN ALPHA.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void InsertMassive<T>(this IEnumerable<T> list) where T : Cope<T>, IManageable, new()
        {
            if (list.Count() > 0)
            {
                Manager<T>.InsertMassive(list, null);
            }
        }

        /// <summary>
        /// Actualiza una coleccion de objetos de forma masiva y en una sola llamada en la base de datos. Aun no integrado al pipeline regular de la aplicacion. ACTUALMENTE EN ALPHA.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void UpdateMassive<T>(this IEnumerable<T> list) where T : Cope<T>, IManageable, new()
        {
            if (list.Count() > 0)
            {
                Manager<T>.UpdateMassive(list, null);
            }
        }

        /// <summary>
        /// Borra una coleccion de objetos de forma masiva y en una sola llamada en la base de datos. Aun no integrado al pipeline regular de la aplicacion. ACTUALMENTE EN ALPHA.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void DeleteMassive<T>(this IEnumerable<T> list) where T : Cope<T>, IManageable, new()
        {
            if (list.Count() > 0)
            {
                Manager<T>.DeleteMassive(list, null);
            }
        }
    }
}
