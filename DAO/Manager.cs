using DataManagement.BO;
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
            return Command(obj, DbOperation.TransactionTypes.Insert, useAppConfig);
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result> InsertAsync(T obj, bool useAppConfig)
        {
            return await Task.Run(() => Command(obj, DbOperation.TransactionTypes.Insert, useAppConfig));
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static Result Update(T obj, bool useAppConfig)
        {
            return Command(obj, DbOperation.TransactionTypes.Update, useAppConfig);
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static async Task<Result> UpdateAsync(T obj, bool useAppConfig)
        {
            return await Task.Run(() => Command(obj, DbOperation.TransactionTypes.Update, useAppConfig));
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static Result Delete(T obj, bool useAppConfig)
        {
            return Command(obj, DbOperation.TransactionTypes.Delete, useAppConfig);
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static async Task<Result> DeleteAsync(T obj, bool useAppConfig)
        {
            return await Task.Run(() => Command(obj, DbOperation.TransactionTypes.Delete, useAppConfig));
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result Select(bool useAppConfig, params Parameter[] parameters)
        {
            return Command(DataSerializer.SetParametersInObject<T>(parameters), DbOperation.TransactionTypes.Select, useAppConfig);
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result> SelectAsync(bool useAppConfig, params Parameter[] parameters)
        {
            return await Task.Run(() => Command(DataSerializer.SetParametersInObject<T>(parameters), DbOperation.TransactionTypes.Select, useAppConfig));
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
            DbOperation.ConnectionTypes connectionType = (DbOperation.ConnectionTypes)Enum.Parse(typeof(DbOperation.ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
            DbOperation dbOperation = connectionType == DbOperation.ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();

            return dbOperation.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig);
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
            DbOperation.ConnectionTypes connectionType = (DbOperation.ConnectionTypes)Enum.Parse(typeof(DbOperation.ConnectionTypes), ConfigurationManager.AppSettings["ConnectionType"].ToString());
            DbOperation dbOperation = connectionType == DbOperation.ConnectionTypes.MySQL ? (DbOperation)new MySqlOperation() : (DbOperation)new MsSqlOperation();

            return await Task.Run(() => dbOperation.EjecutarProcedimiento(tableName, storedProcedure, parameters, useAppConfig));
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result SelectAll(bool useAppConfig)
        {
            return Command(new T(), DbOperation.TransactionTypes.SelectAll, useAppConfig);
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="useAppConfig">Señala si se debe de usar el archivo de configuracion para conectarse a la base de datos.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result> SelectAllAsync(bool useAppConfig)
        {
            return await Task.Run(() => Command(new T(), DbOperation.TransactionTypes.SelectAll, useAppConfig));
        }

        private static Result Command(T obj, DbOperation.TransactionTypes transactionType, bool useAppConfig)
        {
            QueryEvaluation queryEvaluation = new QueryEvaluation();
            Result resultado;
            if (dataCache.IsCacheEnabled)
            {
                ResetCacheIfExpired();
                resultado = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, forceQueryDataBase, useAppConfig);
                SaveCache(transactionType, resultado);
            }
            else
            {
                // Al mandar TRUE en forceQueryDataBase aseguramos que no se use el cache y al no almacenar el resultado con la funcion SaveCache, anulamos completamente el uso cache.
                resultado = queryEvaluation.Evaluate(obj, transactionType, dataCache.Cache, dataCache.IsPartialCache, true, useAppConfig);
            }

            return resultado;
        }

        private static void SaveCache(DbOperation.TransactionTypes transactionType, Result resultado)
        {
            if (dataCache.Cache == null || dataCache.IsPartialCache)
            {
                // Cada vez que actualizamos el cache se debe de actualizar la variable para determinar cuando fue la ultima vez que se actualizo el cache
                dataCache.LastCacheUpdate = DateTime.Now.Ticks;

                forceQueryDataBase = false;

                if (transactionType == DbOperation.TransactionTypes.SelectAll)
                {
                    dataCache.Cache = resultado;
                    dataCache.IsPartialCache = false;
                }
                else if (resultado.Data.Rows.Count > 0 && transactionType == DbOperation.TransactionTypes.Select)
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
                else if (transactionType == DbOperation.TransactionTypes.Insert)
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
    }
}
