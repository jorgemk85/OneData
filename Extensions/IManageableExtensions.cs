using OneData.DAO;
using OneData.Interfaces;
using OneData.Models;
using OneData.Models.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OneData.Extensions
{
    public static class IManageableExtensions
    {
        /// <summary>
        /// Borra el objeto en la base de datos segun su Id y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Delete<T>(this T obj) where T : IManageable, new()
        {
            Manager<T>.Delete(obj, null);
        }

        /// <summary>
        /// Actualiza el objeto en la base de datos y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <param name="updateNulls">Indica si se desea actualizar nulos o ser ignorados durante la consulta.</param>
        public static void Update<T>(this T obj, bool updateNulls = false) where T : IManageable, new()
        {
            Manager<T>.Update(obj, new QueryOptions() { UpdateNulls = updateNulls });
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. 
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Insert<T>(this T obj, bool doValidation = false) where T : IManageable, new()
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T>.Insert(obj, null);
        }
    }
}
