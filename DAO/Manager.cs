﻿using DataManagement.Enums;
using DataManagement.Events;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DataManagement.DAO
{
    /// <summary>
    /// Clase estatica donde se procesan las ejecuciones de procedimientos almacenados y se guardan propiedades predeterminadas de uso interno.
    /// </summary>
    public static class Manager
    {
        internal static string DefaultSchema { get; set; }
        internal static string DefaultConnection { get; set; }
        internal static ConnectionTypes ConnectionType { get; set; }
        internal static bool IsDebug { get; set; }
        internal static bool AutoCreateStoredProcedures { get; set; }
        internal static bool AutoCreateTables { get; set; }
        internal static bool AutoAlterStoredProcedures { get; set; }
        internal static bool AutoAlterTables { get; set; }
        internal static bool EnableLogInDatabase { get; set; }
        internal static bool EnableLogInFile { get; set; }
        internal static bool ConstantTableConsolidation { get; set; }
        internal static bool OverrideOnlyInDebug { get; set; }
        internal static string SelectSuffix { get; set; }
        internal static string InsertSuffix { get; set; }
        internal static string InsertMassiveSuffix { get; set; }
        internal static string UpdateSuffix { get; set; }
        internal static string DeleteSuffix { get; set; }
        internal static string SelectAllSuffix { get; set; }
        internal static string StoredProcedurePrefix { get; set; }
        internal static string TablePrefix { get; set; }

        static Manager()
        {
            GetConfigurationSettings();
            GetPrefixesAndSuffixes();
        }

        internal static void GetConfigurationSettings()
        {
            SetIfDebug();
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
            OverrideOnlyInDebug = bool.Parse(ConsolidationTools.GetValueFromConfiguration("OverrideOnlyInDebug", ConfigurationTypes.AppSetting));
            Logger.Info("Got Manager configuration settings.");
        }

        internal static void GetPrefixesAndSuffixes()
        {
            Logger.Info("Getting Manager configuration for prefixes and suffixes.");
            SelectSuffix = ConsolidationTools.GetValueFromConfiguration("SelectSuffix", ConfigurationTypes.AppSetting);
            InsertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            InsertMassiveSuffix = ConsolidationTools.GetValueFromConfiguration("InsertMassiveSuffix", ConfigurationTypes.AppSetting);
            UpdateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            DeleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
            SelectAllSuffix = ConsolidationTools.GetValueFromConfiguration("SelectAllSuffix", ConfigurationTypes.AppSetting);
            StoredProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);
            TablePrefix = ConsolidationTools.GetValueFromConfiguration("TablePrefix", ConfigurationTypes.AppSetting);
        }

        internal static void SetIfDebug()
        {
#if DEBUG
            IsDebug = true;
            Logger.Info("Debug mode is set.");
#else
            IsDebug = false;
            Logger.Info("Release mode is set.");
#endif
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo DataSet que contiene la informacion resultante de la ejecucion.</returns>
        public static DataSet StoredProcedure(string tableName, string storedProcedure, string connectionToUse = null, params Parameter[] parameters)
        {
            return ExecuteStoredProcedure(tableName, storedProcedure, connectionToUse, parameters);
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos usando los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="tableName">Nombre de la tabla relacionada al procedimiento almacenado. Este dato es solo para referencia al crear el DataTable.</param>
        /// <param name="storedProcedure">Nombre exacto del procedimiento almacenado a ejecutar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la ejecucion.</returns>
        public static async Task<DataSet> StoredProcedureAsync(string tableName, string storedProcedure, string connectionToUse = null, params Parameter[] parameters)
        {
            return await Task.Run(() => ExecuteStoredProcedure(tableName, storedProcedure, connectionToUse, parameters));
        }

        private static DataSet ExecuteStoredProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters)
        {
            if (connectionToUse == null) connectionToUse = DefaultConnection;
            IOperable operation = Operation.GetOperationBasedOnConnectionType(ConnectionType);
            return operation.ExecuteProcedure(tableName, storedProcedure, connectionToUse, parameters);
        }
    }

    /// <summary>
    /// Clase sellada donde se procesan las consultas a la base de datos y se administra el cache.
    /// </summary>
    /// <typeparam name="T">Tipo de clase que representa este objeto. El tipo tiene que implementar IManageable para poder operar.</typeparam>
    public sealed class Manager<T> where T : Cope<T>, IManageable, new()
    {
        static DataCache<T> dataCache = new DataCache<T>();
        static readonly ModelComposition _modelComposition = new ModelComposition(typeof(T));
        static readonly Composition _composition = new Composition();

        internal static ref readonly ModelComposition ModelComposition => ref _modelComposition;
        internal static ref readonly Composition Composition => ref _composition;

        #region Events
        public static event CommandExecutedEventHandler<T> OnCommandExecuted;
        public static event SelectExecutedEventHandler<T> OnSelectExecuted;
        public static event SelectAllExecutedEventHandler<T> OnSelectAllExecuted;
        public static event DeleteExecutedEventHandler<T> OnDeleteExecuted;
        public static event InsertExecutedEventHandler<T> OnInsertExecuted;
        public static event InsertMassiveExecutedEventHandler<T> OnInsertMassiveExecuted;
        public static event UpdateExecutedEventHandler<T> OnUpdateExecuted;
        public static event StoredProcedureExecutedEventHandler<T> OnStoredProcedureExecuted;
        #endregion

        static Manager()
        {
            dataCache.Initialize(ModelComposition.IsCacheEnabled);
            SetupComposition();
        }

        private static void SetupComposition()
        {
            _composition.CacheExpiration = _modelComposition.CacheExpiration;
            _composition.ForeignPrimaryKeyName = _modelComposition.ForeignPrimaryKeyName;
            _composition.IsCacheEnabled = _modelComposition.IsCacheEnabled;
            _composition.Schema = _modelComposition.Schema;
            _composition.TableName = _modelComposition.TableName;
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result<T> Insert(T obj, string connectionToUse = null)
        {
            return Command(obj, TransactionTypes.Insert, connectionToUse);
        }

        /// <summary>
        /// Inserta una lista de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="list">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static Result<T> InsertMassive(IEnumerable<T> list, string connectionToUse = null)
        {
            return Command(list, TransactionTypes.InsertMassive, connectionToUse);
        }

        /// <summary>
        /// Inserta un objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion a insertar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la insercion.</returns>
        public static async Task<Result<T>> InsertAsync(T obj, string connectionToUse = null)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Insert, connectionToUse));
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static Result<T> Update(T obj, string connectionToUse = null)
        {
            return Command(obj, TransactionTypes.Update, connectionToUse);
        }

        /// <summary>
        /// Actualiza el objeto de tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene la informacion actualizada.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la actualizacion.</returns>
        public static async Task<Result<T>> UpdateAsync(T obj, string connectionToUse = null)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Update, connectionToUse));
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static Result<T> Delete(T obj, string connectionToUse = null)
        {
            return Command(obj, TransactionTypes.Delete, connectionToUse);
        }

        /// <summary>
        /// Elimina el objeto de tipo <typeparamref name="T"/> en la base de datos Async.
        /// </summary>
        /// <param name="obj">Objeto que contiene el Id a eliminar.</param>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la eliminacion.</returns>
        public static async Task<Result<T>> DeleteAsync(T obj, string connectionToUse = null)
        {
            return await Task.Run(() => Command(obj, TransactionTypes.Delete, connectionToUse));
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result<T> Select(string connectionToUse = null, params Parameter[] parameters)
        {
            return Command(DataSerializer.SetParametersInObject<T>(parameters), TransactionTypes.Select, connectionToUse);
        }

        /// <summary>
        /// Ejecuta una consulta de seleccion en la base de datos usando el objeto de tipo <typeparamref name="T"/> como referencia y los parametros proporcionados usando Async.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <param name="parameters">Formacion de objetos Parameter que contiene los parametros de la consulta.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result<T>> SelectAsync(string connectionToUse = null, params Parameter[] parameters)
        {
            return await Task.Run(() => Command(DataSerializer.SetParametersInObject<T>(parameters), TransactionTypes.Select, connectionToUse));
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static Result<T> SelectAll(string connectionToUse = null)
        {
            return Command(new T(), TransactionTypes.SelectAll, connectionToUse);
        }

        /// <summary>
        /// Seleccion de todos los objetos del tipo <typeparamref name="T"/> en la base de datos usando Async.
        /// </summary>
        /// <param name="connectionToUse">Especifica cual configuracion de tipo ConectionString se desea utilizar. Si se especifica nulo, o no se especifica, entonces utiliza la conexion especificada en DefaultConnection.</param>
        /// <returns>Regresa un nuevo objeto Result que contiene la informacion resultante de la seleccion.</returns>
        public static async Task<Result<T>> SelectAllAsync(string connectionToUse = null)
        {
            return await Task.Run(() => Command(new T(), TransactionTypes.SelectAll, connectionToUse));
        }

        private static Result<T> Command(T obj, TransactionTypes transactionType, string connectionToUse = null)
        {
            return DynamicCommand(obj, transactionType, connectionToUse);
        }

        private static Result<T> Command(IEnumerable<T> list, TransactionTypes transactionType, string connectionToUse = null)
        {
            return DynamicCommand(list, transactionType, connectionToUse);
        }

        private static Result<T> DynamicCommand(dynamic obj, TransactionTypes transactionType, string connectionToUse = null)
        {
            if (connectionToUse == null) connectionToUse = Manager.DefaultConnection;
            QueryEvaluation queryEvaluation = new QueryEvaluation(Manager.ConnectionType);

            if (ModelComposition.IsCacheEnabled)
            {
                ResetCacheIfExpired();
            }

            Result<T> result = queryEvaluation.Evaluate<T>(obj, transactionType, ref dataCache, connectionToUse);
            CallOnExecutedEventHandlers(ModelComposition.TableName, transactionType, result);

            return result;
        }

        private static void ResetCacheIfExpired()
        {
            if (DateTime.Now.Ticks > dataCache.LastCacheUpdate + ModelComposition.CacheExpiration)
            {
                // Elimina el cache ya que esta EXPIRADO y de debe de refrescar.
                dataCache.Reset(ModelComposition.IsCacheEnabled);
            }
        }

        private static void CallOnExecutedEventHandlers(string tableName, TransactionTypes transactionType, Result<T> result)
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    OnSelectExecuted?.Invoke(new SelectExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.SelectAll:
                    OnSelectAllExecuted?.Invoke(new SelectAllExecutedEventArgs<T>(tableName, result));
                    break;
                case TransactionTypes.Delete:
                    OnDeleteExecuted?.Invoke(new DeleteExecutedEventArgs<T>(tableName, result));
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
                case TransactionTypes.StoredProcedure:
                    OnStoredProcedureExecuted?.Invoke(new StoredProcedureExecutedEventArgs<T>(tableName, result));
                    break;
                default:
                    break;
            }
            OnCommandExecuted?.Invoke(new CommandExecutedEventArgs<T>(tableName, result));
        }
    }
}
