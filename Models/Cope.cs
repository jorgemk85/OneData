using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Enums;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using System;
using System.Collections.Generic;

namespace DataManagement.Models
{
    /// <summary>
    /// Clase principal de la que tienen que heredar todos los objetos de negocio que se desee utilizar con la libreria DataManagement.
    /// </summary>
    /// <typeparam name="T">Representa el tipo de la clase que esta heredando de Cope<T, TKey>.</typeparam>
    /// <typeparam name="TKey">Representa el tipo a utilizar para la llave primaria del Id.</typeparam>
    [Serializable]
    public abstract class Cope<T, TKey> : IEquatable<T>, IManageable<TKey> where T : Cope<T, TKey>, new() where TKey : struct
    {
        #region Fields
        private readonly string _foreignIdName = string.Format("{0}Id", typeof(T).Name);
        #endregion

        #region Primary Property
        [PrimaryProperty]
        public TKey? Id { get; set; }
        #endregion

        #region Auto Properties
        [DateCreatedProperty, AutoProperty(AutoPropertyTypes.DateTime)]
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;
        [DateModifiedProperty, AutoProperty(AutoPropertyTypes.DateTime)]
        public DateTime? FechaModificacion { get; set; } = DateTime.Now;
        #endregion

        #region Unmanaged Properties
        [UnmanagedProperty]
        public ModelComposition ModelComposition { get; } = Manager<T, TKey>.ModelComposition;

        [UnmanagedProperty]
        public string ForeignIdName { get; } = string.Format("{0}Id", typeof(T).Name);
        #endregion

        #region Static Methods
        /// <summary>
        /// Borra el objeto en la base de datos segun su Id y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Delete(T obj)
        {
            Manager<T, TKey>.Delete(obj);
        }

        /// <summary>
        /// Actualiza el objeto en la base de datos y en su supuesto, tambien en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <param name="doValidation">Indica si se desea realizar la validacion de nulos.</param>
        public static void Update(T obj, bool doValidation = false)
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T, TKey>.Update(obj);
        }

        /// <summary>
        /// Inserta el objeto en la base de datos y en su supuesto, tambien en el cache. 
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        public static void Insert(T obj, bool doValidation = false)
        {
            if (doValidation)
            {
                obj.Validate();
            }
            Manager<T, TKey>.Insert(obj);
        }

        /// <summary>
        /// Obtiene un listado completo de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa la coleccion obtenida ya convertida en una lista del tipo <typeparamref name="T"/></returns>
        public static List<T> SelectAll()
        {
            return Manager<T, TKey>.SelectAll().Dictionary.ToList();
        }

        /// <summary>
        /// Obtiene el primer objeto de tipo <typeparamref name="T"/> almacenado en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static T Select(params Parameter[] parameters)
        {
            return Manager<T, TKey>.Select(null, parameters).Dictionary.ToObject();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa la coleccion obtenida ya convertida en una lista del tipo <typeparamref name="T"/></returns>
        public static List<T> SelectList(params Parameter[] parameters)
        {
            return Manager<T, TKey>.Select(null, parameters).Dictionary.ToList();
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public static Result<T, TKey> SelectResult(params Parameter[] parameters)
        {
            return Manager<T, TKey>.Select(null, parameters);
        }
        #endregion

        #region Instance Methods
        internal T Include(IManageable<TKey> obj, Type target)
        {
            IManageable<TKey> foreignObject = (IManageable<TKey>)Activator.CreateInstance(target);
            Result<dynamic, TKey> result = foreignObject.GetResultFromSelect(new Parameter(obj.ForeignIdName, obj.Id));
            foreach (KeyValuePair<string, ForeignCollection> attribute in obj.ModelComposition.ForeignCollectionAttributes)
            {
                if (attribute.Value.Model.Equals(target))
                {
                    obj.GetType().GetProperty(attribute.Key).SetValue(obj, result.Dictionary);
                }
            }
            return (T)obj;
        }

        /// <summary>
        /// Obtiene un listado de los objetos de tipo <typeparamref name="T"/> almacenados en la base de datos o en el cache segun los parametros indicados.
        /// Este metodo usa la conexion predeterminada a la base de datos.
        /// </summary>
        /// <returns>Regresa el resultado que incluye la coleccion obtenida por la consulta.</returns>
        public Result<dynamic, TKey> GetResultFromSelect(params Parameter[] parameters)
        {
            return Manager<T, TKey>.Select(null, parameters);
        }

        public bool Equals(T other)
        {
            if (other.Id.Equals(Id))
            {
                return true;
            }
            return false;
        }
        #endregion


        #region Constructors
        // TODO: Necesitamos encontrar una manera mas facil de instanciar objetos con nuevos Ids irrepetibles. 
        public Cope(TKey id)
        {
            Id = id;
        }
        #endregion
    }
}
