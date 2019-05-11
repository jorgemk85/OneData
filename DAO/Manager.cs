using OneData.Enums;
using OneData.Events;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OneData.DAO
{
    /// <summary>
    /// Clase estatica donde se procesan las ejecuciones de procedimientos almacenados y se guardan propiedades predeterminadas de uso interno.
    /// </summary>
    public static class Manager
    {
        public static string DefaultSchema { get; internal set; }
        public static string DefaultConnection { get; internal set; }
        public static ConnectionTypes ConnectionType { get; internal set; }
        public static bool AutoCreateStoredProcedures { get; internal set; }
        public static bool AutoCreateTables { get; internal set; }
        public static bool AutoAlterStoredProcedures { get; internal set; }
        public static bool AutoAlterTables { get; internal set; }
        public static bool EnableLogInDatabase { get; internal set; }
        public static bool EnableLogInFile { get; internal set; }
        public static bool ConstantTableConsolidation { get; internal set; }
        public static string InsertSuffix { get; internal set; }
        public static string UpdateSuffix { get; internal set; }
        public static string DeleteSuffix { get; internal set; }
        public static string StoredProcedurePrefix { get; internal set; }
        public static string TablePrefix { get; internal set; }
        public static IManageable Identity { get; set; }

        static Manager()
        {
            GetConfigurationSettings();
            GetPrefixesAndSuffixes();
        }

        internal static void GetConfigurationSettings()
        {
            DefaultConnection = ConsolidationTools.GetValueFromConfiguration("DefaultConnection", ConfigurationTypes.AppSetting);
            DefaultSchema = ConsolidationTools.GetValueFromConfiguration("DefaultSchema", ConfigurationTypes.AppSetting);
            ConnectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), ConsolidationTools.GetValueFromConfiguration("ConnectionType", ConfigurationTypes.AppSetting));
            AutoCreateStoredProcedures = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateStoredProcedures", ConfigurationTypes.AppSetting));
            AutoCreateTables = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateTables", ConfigurationTypes.AppSetting));
            EnableLogInDatabase = bool.Parse(ConsolidationTools.GetValueFromConfiguration("EnableLogInDatabase", ConfigurationTypes.AppSetting));
            EnableLogInFile = bool.Parse(ConsolidationTools.GetValueFromConfiguration("EnableLogInFile", ConfigurationTypes.AppSetting));
            ConstantTableConsolidation = bool.Parse(ConsolidationTools.GetValueFromConfiguration("ConstantTableConsolidation", ConfigurationTypes.AppSetting));
            AutoAlterStoredProcedures = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoAlterStoredProcedures", ConfigurationTypes.AppSetting));
            AutoAlterTables = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoAlterTables", ConfigurationTypes.AppSetting));
            Logger.Info("Got Manager configuration settings.");
        }

        internal static void GetPrefixesAndSuffixes()
        {
            Logger.Info("Getting Manager configuration for prefixes and suffixes.");
            InsertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            UpdateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            DeleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
            StoredProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);
            TablePrefix = ConsolidationTools.GetValueFromConfiguration("TablePrefix", ConfigurationTypes.AppSetting);
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo DataSet que contiene la informacion resultante de la ejecucion.</returns>
        public static DataSet StoredProcedure(string tableName, string storedProcedure, QueryOptions queryOptions, params Parameter[] parameters)
        {
            return ExecuteStoredProcedure(tableName, storedProcedure, queryOptions, parameters);
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la ejecucion.</returns>
        public static async Task<DataSet> StoredProcedureAsync(string tableName, string storedProcedure, QueryOptions queryOptions, params Parameter[] parameters)
        {
            return await Task.Run(() => ExecuteStoredProcedure(tableName, storedProcedure, queryOptions, parameters));
        }

        private static DataSet ExecuteStoredProcedure(string tableName, string storedProcedure, QueryOptions queryOptions, Parameter[] parameters)
        {
            if (queryOptions == null)
            {
                queryOptions = new QueryOptions() { ConnectionToUse = DefaultConnection };
            }
            if (queryOptions.ConnectionToUse == null)
            {
                queryOptions.ConnectionToUse = DefaultConnection;
            }

            IOperable operation = Operation.GetOperationBasedOnConnectionType(ConnectionType);
            return operation.ExecuteProcedure(tableName, storedProcedure, queryOptions, parameters);
        }
    }

    /// <summary>
    /// Clase sellada donde se procesan las consultas a la base de datos y se administra el cache.
    /// </summary>
    /// <typeparam name="T">Tipo de clase que representa este objeto. El tipo tiene que implementar IManageable para poder operar.</typeparam>
    public sealed class Manager<T> where T : Cope<T>, IManageable, new()
    {
        static DataCache<T> dataCache = new DataCache<T>();

        #region Events
        public static event CommandExecutedEventHandler<T> OnCommandExecuted;
        public static event SelectExecutedEventHandler<T> OnSelectExecuted;
        public static event SelectQueryExecutedEventHandler<T> OnSelectQueryExecuted;
        public static event SelectAllExecutedEventHandler<T> OnSelectAllExecuted;
        public static event DeleteExecutedEventHandler<T> OnDeleteExecuted;
        public static event DeleteMassiveExecutedEventHandler<T> OnDeleteMassiveExecuted;
        public static event InsertExecutedEventHandler<T> OnInsertExecuted;
        public static event InsertMassiveExecutedEventHandler<T> OnInsertMassiveExecuted;
        public static event UpdateExecutedEventHandler<T> OnUpdateExecuted;
        public static event UpdateMassiveExecutedEventHandler<T> OnUpdateMassiveExecuted;
        public static event StoredProcedureExecutedEventHandler<T> OnStoredProcedureExecuted;
        #endregion

        static Manager()
        {
            dataCache.Initialize(Cope<T>.ModelComposition.IsCacheEnabled);
        }

        private static void VerifyQueryOptions(ref QueryOptions queryOptions)
        {
            if (queryOptions == null)
            {
                queryOptions = new QueryOptions();
                queryOptions.MaximumResults = -1;
                queryOptions.Offset = 0;
                queryOptions.ConnectionToUse = Manager.DefaultConnection;
            }
            else
            {
                queryOptions.MaximumResults = queryOptions.MaximumResults < -1 ? -1 : queryOptions.MaximumResults;
                queryOptions.Offset = queryOptions.MaximumResults < 0 ? 0 : queryOptions.Offset;
                queryOptions.ConnectionToUse = string.IsNullOrWhiteSpace(queryOptions.ConnectionToUse) ? Manager.DefaultConnection : queryOptions.ConnectionToUse;
            }
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result<T> Insert(T obj, QueryOptions queryOptions)
        {
            return Command(obj, TransactionTypes.Insert, queryOptions);
        }

        /// <summary>
        /// Inserta una lista de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="list">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result<T> InsertMassive(IEnumerable<T> list, QueryOptions queryOptions)
        {
            return Command(list, TransactionTypes.InsertMassive, queryOptions);
        }

        /// <summary>
        /// Actualiza una lista de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="list">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result<T> UpdateMassive(IEnumerable<T> list, QueryOptions queryOptions)
        {
            return Command(list, TransactionTypes.UpdateMassive, queryOptions);
        }

        /// <summary>
        /// Borra una lista de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="list">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result<T> DeleteMassive(IEnumerable<T> list, QueryOptions queryOptions)
        {
            return Command(list, TransactionTypes.DeleteMassive, queryOptions);
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result<T>> InsertAsync(T obj, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Insert, queryOptions));
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result<T>> InsertMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(list, TransactionTypes.InsertMassive, queryOptions));
        }

        /// <summary>
        /// Actualiza un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result<T>> UpdateMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(list, TransactionTypes.UpdateMassive, queryOptions));
        }

        /// <summary>
        /// Borra un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result<T>> DeleteMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(list, TransactionTypes.DeleteMassive, queryOptions));
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static Result<T> Update(T obj, QueryOptions queryOptions)
        {
            return Command(obj, TransactionTypes.Update, queryOptions);
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static async Task<Result<T>> UpdateAsync(T obj, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Update, queryOptions));
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static Result<T> Delete(T obj, QueryOptions queryOptions)
        {
            return Command(obj, TransactionTypes.Delete, queryOptions);
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static async Task<Result<T>> DeleteAsync(T obj, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Delete, queryOptions));
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados en forma de una expresion lambda.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="expression">Expresion Lambda que represnta un resultado verdadero de un boolean utilizado para la conversion a SQL y la execucion de la declaracion Select.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result<T> Select(Expression<Func<T, bool>> expression, QueryOptions queryOptions)
        {
            return Command(expression, TransactionTypes.Select, queryOptions);
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados en forma de una expresion lambda.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="expression">Expresion Lambda que represnta un resultado verdadero de un boolean utilizado para la conversion a SQL y la execucion de la declaracion Select.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result<T>> SelectAsync(Expression<Func<T, bool>> expression, QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(expression, TransactionTypes.Select, queryOptions));
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result<T> SelectAll(QueryOptions queryOptions)
        {
            return Command(new T(), TransactionTypes.SelectAll, queryOptions);
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result<T>> SelectAllAsync(QueryOptions queryOptions)
        {
            return await Task.Run(() => Command(new T(), TransactionTypes.SelectAll, queryOptions));
        }

        private static Result<T> Command(Expression<Func<T, bool>> expression, TransactionTypes transactionType, QueryOptions queryOptions)
        {
            return DynamicCommand(null, expression, transactionType, queryOptions);
        }

        private static Result<T> Command(T obj, TransactionTypes transactionType, QueryOptions queryOptions)
        {
            return DynamicCommand(obj, null, transactionType, queryOptions);
        }

        private static Result<T> Command(IEnumerable<T> list, TransactionTypes transactionType, QueryOptions queryOptions)
        {
            return DynamicCommand(list, null, transactionType, queryOptions);
        }

        private static Result<T> DynamicCommand(dynamic obj, Expression<Func<T, bool>> expression, TransactionTypes transactionType, QueryOptions queryOptions)
        {
            VerifyQueryOptions(ref queryOptions);

            QueryEvaluation queryEvaluation = new QueryEvaluation(Manager.ConnectionType);

            if (Cope<T>.ModelComposition.IsCacheEnabled)
            {
                ResetCacheIfExpired();
            }

            Result<T> result = queryEvaluation.Evaluate<T>(obj, expression, transactionType, ref dataCache, queryOptions);
            CallOnExecutedEventHandlers(Cope<T>.ModelComposition.TableName, transactionType, result);

            return result;
        }

        private static void ResetCacheIfExpired()
        {
            if (DateTime.Now.Ticks > dataCache.LastCacheUpdate + Cope<T>.ModelComposition.CacheExpiration)
            {
                // Elimina el cache ya que esta EXPIRADO y de debe de refrescar.
                dataCache.Reset(Cope<T>.ModelComposition.IsCacheEnabled);
            }
        }

        private static void CallOnExecutedEventHandlers(string tableName, TransactionTypes transactionType, Result<T> result)
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    OnSelectQueryExecuted?.Invoke(new SelectQueryExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.SelectAll:
                    OnSelectAllExecuted?.Invoke(new SelectAllExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.Delete:
                    OnDeleteExecuted?.Invoke(new DeleteExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.DeleteMassive:
                    OnDeleteMassiveExecuted?.Invoke(new DeleteMassiveExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.Insert:
                    OnInsertExecuted?.Invoke(new InsertExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.InsertMassive:
                    OnInsertMassiveExecuted?.Invoke(new InsertMassiveExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.Update:
                    OnUpdateExecuted?.Invoke(new UpdateExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.UpdateMassive:
                    OnUpdateMassiveExecuted?.Invoke(new UpdateMassiveExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.StoredProcedure:
                    OnStoredProcedureExecuted?.Invoke(new StoredProcedureExecutedEventArgs<T>(tableName, result));
                    break;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
            }
            OnCommandExecuted?.Invoke(new CommandExecutedEventArgs<T>(tableName, result));
        }
    }
}
