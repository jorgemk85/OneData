using DataManagement.Enums;
using DataManagement.Events;
using DataManagement.Exceptions;
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
    /// <typeparam name="T">Tipo de clase que representa este objeto.</typeparam>
    public abstract class Manager<T> where T : new()
    {
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
            dataCache.Initialize(new T());
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result Insert(T obj, bool useAppConfig)
        {
            return Command(obj, TransactionTypes.Insert, useAppConfig);
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result> InsertAsync(T obj, bool useAppConfig)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Insert, useAppConfig));
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static Result Update(T obj, bool useAppConfig)
        {
            return Command(obj, TransactionTypes.Update, useAppConfig);
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static async Task<Result> UpdateAsync(T obj, bool useAppConfig)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Update, useAppConfig));
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static Result Delete(T obj, bool useAppConfig)
        {
            return Command(obj, TransactionTypes.Delete, useAppConfig);
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static async Task<Result> DeleteAsync(T obj, bool useAppConfig)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Delete, useAppConfig));
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result Select(bool useAppConfig, params Parameter[] parameters)
        {
            return Command(DataSerializer.SetParametersInObject<T>(parameters), TransactionTypes.Select, useAppConfig);
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result> SelectAsync(bool useAppConfig, params Parameter[] parameters)
        {
            return await Task.Run(() => Command(DataSerializer.SetParametersInObject<T>(parameters), TransactionTypes.Select, useAppConfig));
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la ejecucion.</returns>
        public static Result Select(string tableName, string storedProcedure, bool useAppConfig, params Parameter[] parameters)
        {
            try
            {
                ConnectionTypes connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
                DbOperation dbOperation = connectionType == ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();
                Result result = dbOperation.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
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
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la ejecucion.</returns>
        public static async Task<Result> SelectAsync(string tableName, string storedProcedure, bool useAppConfig, params Parameter[] parameters)
        {
            try
            {
                ConnectionTypes connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
                DbOperation dbOperation = connectionType == ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();
                Result result = await Task.Run(() => dbOperation.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig));
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
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result SelectAll(bool useAppConfig)
        {
            return Command(new T(), TransactionTypes.SelectAll, useAppConfig);
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result> SelectAllAsync(bool useAppConfig)
        {
            return await Task.Run(() => Command(new T(), TransactionTypes.SelectAll, useAppConfig));
        }

        private static Result Command(T obj, TransactionTypes transactionType, bool useAppConfig)
        {
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result result;
            if (dataCache.IsCacheEnabled)
            {
                ResetCacheIfExpired();
                result = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, forceQueryDataBase, useAppConfig);
                SaveCache(transactionType, result);
            }
            else
            {
                // Al mandar TRUE en forceQueryDataBase aseguramos que no se use el cache y al no almacenar el resultado con la funcion SaveCache, anulamos completamente el uso cache.
                result = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, true, useAppConfig);
            }
            CallOnExecutedEventHandlers((obj as Main).DataBaseTableName, transactionType, result);
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
