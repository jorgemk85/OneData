using DataManagement.Enums;
using DataManagement.Events;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace DataManagement.DAO
{
    /// <summary>
    /// Clase abstracta donde se procesan las consultas a la base de datos y se administra el cache.
    /// </summary>
    /// <typeparam name="T">Tipo de clase que representa este objeto. El tipo tiene que implementar IManageable para poder operar.</typeparam>
    public abstract class Manager<T> where T : IManageable, new()
    {
        static string defaultConnection;
        static DataCache dataCache = new DataCache();
        static bool forceQueryDataBase = false;

        #region Events
        public static event CommandExecutedEventHandler OnCommandExecuted;
        public static event SelectExecutedEventHandler OnSelectExecuted;
        public static event SelectAllExecutedEventHandler OnSelectAllExecuted;
        public static event DeleteExecutedEventHandler OnDeleteExecuted;
        public static event InsertExecutedEventHandler OnInsertExecuted;
        public static event UpdateExecutedEventHandler OnUpdateExecuted;
        public static event StoredProcedureExecutedEventHandler OnStoredProcedureExecuted;
        #endregion

        static Manager()
        {
            SetDefaultConnectionName();
            dataCache.Initialize(new T());
        }

        private static void SetDefaultConnectionName()
        {
            try
            {
                defaultConnection = ConsolidationTools.GetValueFromConfiguration("DefaultConnection", ConfigurationTypes.AppSetting);
            }
            catch (ConfigurationErrorsException cee)
            {
                throw cee;
            }
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result Insert(T obj, string connectionToUse = null)
        {
            return Command(obj, TransactionTypes.Insert, connectionToUse);
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result> InsertAsync(T obj, string connectionToUse = null)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Insert, connectionToUse));
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static Result Update(T obj, string connectionToUse = null)
        {
            return Command(obj, TransactionTypes.Update, connectionToUse);
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static async Task<Result> UpdateAsync(T obj, string connectionToUse = null)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Update, connectionToUse));
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static Result Delete(T obj, string connectionToUse = null)
        {
            return Command(obj, TransactionTypes.Delete, connectionToUse);
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static async Task<Result> DeleteAsync(T obj, string connectionToUse = null)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Delete, connectionToUse));
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result Select(string connectionToUse = null, params Parameter[] parameters)
        {
            return Command(DataSerializer.SetParametersInObject<T>(parameters), TransactionTypes.Select, connectionToUse);
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result> SelectAsync(string connectionToUse = null, params Parameter[] parameters)
        {
            return await Task.Run(() => Command(DataSerializer.SetParametersInObject<T>(parameters), TransactionTypes.Select, connectionToUse));
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la ejecucion.</returns>
        public static Result StoredProcedure(string tableName, string storedProcedure, string connectionToUse = null, params Parameter[] parameters)
        {
            try
            {
                if (connectionToUse == null) connectionToUse = defaultConnection;
                ConnectionTypes connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
                Operation dbOperation = connectionType == ConnectionTypes.MySQL ? (Operation)new MySqlOperation() : (Operation)new MsSqlOperation();
                Result result = dbOperation.ExecuteProcedure(tableName, storedProcedure, connectionToUse, parameters);
                CallOnExecutedEventHandlers(tableName, TransactionTypes.StoredProcedure, result);
                return result;
            }
            catch (NullReferenceException nre)
            {
                throw new ConfigurationNotFoundException("ConnectionType", nre);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la ejecucion.</returns>
        public static async Task<Result> StoredProcedureAsync(string tableName, string storedProcedure, string connectionToUse = null, params Parameter[] parameters)
        {
            try
            {
                if (connectionToUse == null) connectionToUse = defaultConnection;
                ConnectionTypes connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
                Operation dbOperation = connectionType == ConnectionTypes.MySQL ? (Operation)new MySqlOperation() : (Operation)new MsSqlOperation();
                Result result = await Task.Run(() => dbOperation.ExecuteProcedure(tableName, storedProcedure, connectionToUse, parameters));
                CallOnExecutedEventHandlers(tableName, TransactionTypes.StoredProcedure, result);
                return result;
            }
            catch (NullReferenceException nre)
            {
                throw new ConfigurationNotFoundException("ConnectionType", nre);
            }
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result SelectAll(string connectionToUse = null)
        {
            return Command(new T(), TransactionTypes.SelectAll, connectionToUse);
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result> SelectAllAsync(string connectionToUse = null)
        {
            return await Task.Run(() => Command(new T(), TransactionTypes.SelectAll, connectionToUse));
        }

        private static Result Command(T obj, TransactionTypes transactionType, string connectionToUse = null)
        {
            if (connectionToUse == null) connectionToUse = defaultConnection;
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result result;
            if (dataCache.IsCacheEnabled)
            {
                ResetCacheIfExpired();
                result = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, forceQueryDataBase, connectionToUse);
                SaveCache(transactionType, result);
            }
            else
            {
                // Al mandar TRUE en forceQueryDataBase aseguramos que no se use el cache y al no almacenar el resultado con la funcion SaveCache, anulamos completamente el uso cache.
                result = queryEvaluation.Evaluate<T>(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, true, connectionToUse);
            }
            CallOnExecutedEventHandlers(obj.DataBaseTableName, transactionType, result);
            return result;
        }

        private static void SaveCache(TransactionTypes transactionType, Result resultado)
        {
            if (dataCache.Cache == null || dataCache.IsPartialCache)
            {
                // Cada vez que actualizamos el cache se debe de actualizar la variable para determinar cuando fue la ultima vez que se actualizo el cache
                dataCache.LastCacheUpdate = DateTime.Now.Ticks;

                forceQueryDataBase = false;

                if (transactionType == TransactionTypes.SelectAll)
                {
                    dataCache.Cache = resultado;
                    dataCache.IsPartialCache = false;
                }
                else if (resultado.Data.Rows.Count > 0 && transactionType == TransactionTypes.Select)
                {
                    if (dataCache.Cache == null)
                    {
                        dataCache.Cache = resultado;
                    }
                    else
                    {
                        if (!resultado.IsFromCache)
                        {
                            QueryEvaluation queryEvaluation = new QueryEvaluation();
                            foreach (DataRow row in resultado.Data.Rows)
                            {
                                queryEvaluation.AlterCache(row, dataCache.Cache);
                            }
                        }
                    }

                    dataCache.IsPartialCache = true;
                }
                else if (transactionType == TransactionTypes.Insert)
                {
                    forceQueryDataBase = true;
                }
            }
        }

        private static void ResetCacheIfExpired()
        {
            if (DateTime.Now.Ticks > dataCache.LastCacheUpdate + dataCache.CacheExpiration)
            {
                // Elimina el cache ya que esta EXPIRADO y de debe de refrescar.
                dataCache.Reset(new T());
            }
        }

        private static void CallOnExecutedEventHandlers(string tableName, TransactionTypes transactionType, Result result)
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    OnSelectExecuted?.Invoke(new SelectExecutedEventArgs(tableName, result));
                    break;
                case TransactionTypes.SelectAll:
                    OnSelectAllExecuted?.Invoke(new SelectAllExecutedEventArgs(tableName, result));
                    break;
                case TransactionTypes.Delete:
                    OnDeleteExecuted?.Invoke(new DeleteExecutedEventArgs(tableName, result));
                    break;
                case TransactionTypes.Insert:
                    OnInsertExecuted?.Invoke(new InsertExecutedEventArgs(tableName, result));
                    break;
                case TransactionTypes.Update:
                    OnUpdateExecuted?.Invoke(new UpdateExecutedEventArgs(tableName, result));
                    break;
                case TransactionTypes.StoredProcedure:
                    OnStoredProcedureExecuted?.Invoke(new StoredProcedureExecutedEventArgs(tableName, result));
                    break;
                default:
                    break;
            }
            OnCommandExecuted?.Invoke(new CommandExecutedEventArgs(tableName, result));
        }
    }
}
