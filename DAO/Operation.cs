﻿using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Extensions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal abstract class Operation
    {
        public string SelectSuffix { get; private set; }
        public string InsertSuffix { get; private set; }
        public string UpdateSuffix { get; private set; }
        public string DeleteSuffix { get; private set; }
        public string SelectAllSuffix { get; private set; }
        public string StoredProcedurePrefix { get; private set; }
        public string TablePrefix { get; private set; }
        public bool AutoCreateStoredProcedures { get; private set; }
        public bool AutoCreateTables { get; private set; }
        public bool AutoAlterStoredProcedures { get; private set; }
        public bool AutoAlterTables { get; private set; }
        public bool EnableLog { get; private set; }
        public bool ConstantTableConsolidation { get; private set; }
        public bool OverrideOnlyInDebug { get; private set; }

        public string QueryForTableExistance { get; protected set; }
        public string QueryForColumnDefinition { get; protected set; }
        public string QueryForKeyDefinition { get; protected set; }
        public ICreatable Creator { get; set; }
        public ConnectionTypes ConnectionType { get; set; }
        public DbCommand Command { get; set; }


        public Operation()
        {
            GetConfigurationSettings();
        }

        protected object ExecuteScalar(string transaction, string connectionToUse, bool returnDataTable)
        {
            if (string.IsNullOrWhiteSpace(transaction))
            {
                return null;
            }

            try
            {
                Logger.Info(string.Format("Starting execution for transaction using connection {0}", connectionToUse));
                using (DbConnection connection = ConnectionType == ConnectionTypes.MySQL ? (DbConnection)Connection.OpenMySqlConnection(connectionToUse) : (DbConnection)Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = transaction;
                    if (returnDataTable)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(Command.ExecuteReader());
                        return dataTable;
                    }
                    else
                    {
                        return Command.ExecuteScalar();
                    }
                }
            }
            catch (DbException dbException)
            {
                if (dbException.InnerException != null)
                {
                    if (dbException.InnerException.Message.EndsWith("must be defined."))
                    {
                        ConnectionVariableNotEnabledException cvnee = new ConnectionVariableNotEnabledException("AllowUserVariables=True");
                        Logger.Error(cvnee);
                        throw cvnee;
                    }
                }

                Logger.Error(dbException);
                throw dbException;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        private void GetConfigurationSettings()
        {
            Logger.Info("Getting Operation configuration settings.");
            SelectSuffix = ConsolidationTools.GetValueFromConfiguration("SelectSuffix", ConfigurationTypes.AppSetting);
            InsertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            UpdateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            DeleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
            SelectAllSuffix = ConsolidationTools.GetValueFromConfiguration("SelectAllSuffix", ConfigurationTypes.AppSetting);
            StoredProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);
            TablePrefix = ConsolidationTools.GetValueFromConfiguration("TablePrefix", ConfigurationTypes.AppSetting);

            AutoCreateStoredProcedures = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateStoredProcedures", ConfigurationTypes.AppSetting));
            AutoCreateTables = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateTables", ConfigurationTypes.AppSetting));
            EnableLog = bool.Parse(ConsolidationTools.GetValueFromConfiguration("EnableLog", ConfigurationTypes.AppSetting));
            ConstantTableConsolidation = bool.Parse(ConsolidationTools.GetValueFromConfiguration("ConstantTableConsolidation", ConfigurationTypes.AppSetting));
            AutoAlterStoredProcedures = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoAlterStoredProcedures", ConfigurationTypes.AppSetting));
            AutoAlterTables = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoAlterTables", ConfigurationTypes.AppSetting));
            OverrideOnlyInDebug = bool.Parse(ConsolidationTools.GetValueFromConfiguration("OverrideOnlyInDebug", ConfigurationTypes.AppSetting));
        }

        internal static IOperable GetOperationBasedOnConnectionType(ConnectionTypes connectionType)
        {
            switch (connectionType)
            {
                case ConnectionTypes.MySQL:
                    return new MySqlOperation();
                case ConnectionTypes.MSSQL:
                    return new MsSqlOperation();
                default:
                    return new MsSqlOperation();
            }
        }

        protected string GetTransactionTextForProcedure<T>(TransactionTypes transactionType, bool doAlter) where T : IManageable, new()
        {
            Logger.Info(string.Format("Getting {0} transaction for type {1}. DoAlter = {2}", transactionType.ToString(), typeof(T), doAlter));
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Creator.CreateSelectStoredProcedure<T>(doAlter);
                case TransactionTypes.SelectAll:
                    return Creator.CreateSelectAllStoredProcedure<T>(doAlter);
                case TransactionTypes.Delete:
                    return Creator.CreateDeleteStoredProcedure<T>(doAlter);
                case TransactionTypes.Insert:
                    return Creator.CreateInsertStoredProcedure<T>(doAlter);
                case TransactionTypes.Update:
                    return Creator.CreateUpdateStoredProcedure<T>(doAlter);
                default:
                    ArgumentException argumentException = new ArgumentException("El tipo de transaccion no es valido para generar un nuevo procedimiento almacenado.");
                    Logger.Error(argumentException);
                    throw argumentException;
            }
        }

        private string GetStringParameters()
        {
            Logger.Info(string.Format("Getting string parameters"));

            StringBuilder builder = new StringBuilder();

            foreach (DbParameter parametro in Command.Parameters)
            {
                if (parametro.Value != null)
                {
                    builder.AppendFormat("{0}: {1}|", parametro.ParameterName, parametro.Value);
                }
            }

            return builder.ToString();
        }

        protected string GetFriendlyTransactionSuffix(TransactionTypes transactionType)
        {
            Logger.Info(string.Format("Getting friendly transaction suffix for transaction type {0}.", transactionType.ToString()));
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return SelectSuffix;
                case TransactionTypes.Delete:
                    return DeleteSuffix;
                case TransactionTypes.Insert:
                    return InsertSuffix;
                case TransactionTypes.Update:
                    return UpdateSuffix;
                case TransactionTypes.SelectAll:
                    return SelectAllSuffix;
                default:
                    return SelectAllSuffix;
            }
        }

        private DbParameter CreateDbParameter(string name, object value)
        {
            DbParameter dbParameter = Command.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            return dbParameter;
        }

        protected void SetParameters(Parameter[] parameters)
        {
            Logger.Info(string.Format("Setting parameters in command."));
            for (int i = 0; i < parameters.Length; i++)
            {
                Command.Parameters.Add(CreateDbParameter(parameters[i].Name, parameters[i].Value));
            }
        }

        protected void SetParameters<T>(T obj, TransactionTypes transactionType)
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo unlinkedProperty o InternalProperty entonces se brinca la propiedad.
                if (propertyInfo.GetCustomAttribute<UnlinkedProperty>() != null) continue;
                if (propertyInfo.GetCustomAttribute<UnmanagedProperty>() != null) continue;

                if (transactionType == TransactionTypes.Delete)
                {
                    if (propertyInfo.Name == "Id")
                    {
                        Command.Parameters.Add(CreateDbParameter("_id", propertyInfo.GetValue(obj)));
                        break;
                    }
                }
                else
                {
                    Command.Parameters.Add(CreateDbParameter("_" + propertyInfo.Name, propertyInfo.GetValue(obj)));
                }
            }
        }

        protected void PerformTableConsolidation<T>(string connectionToUse, bool doAlter) where T : IManageable, new()
        {
            T newObj = new T();
            Logger.Info(string.Format("Starting table consolidation for table {0} using connection {1}. DoAlter = {2}", newObj.DataBaseTableName, connectionToUse, doAlter));
            if (!doAlter)
            {
                if (!CheckIfTableExists(newObj.DataBaseTableName, connectionToUse))
                {
                    ProcessTable<T>(connectionToUse, false);
                    return;
                }
            }
            ExecuteScalar(Creator.GetAlterTableQuery(typeof(T), GetColumnDefinition(newObj.DataBaseTableName, connectionToUse), GetKeyDefinition(newObj.DataBaseTableName, connectionToUse)), connectionToUse, false);
        }

        protected void ProcessTable<T>(string connectionToUse, bool doAlter) where T : IManageable, new()
        {
            Logger.Info(string.Format("Processing table {0} using connection {1}. DoAlter = {2}", new T().DataBaseTableName, connectionToUse, doAlter));
            if (doAlter)
            {
                PerformTableConsolidation<T>(connectionToUse, doAlter);
            }
            else
            {
                ExecuteScalar(Creator.GetCreateTableQuery(typeof(T)), connectionToUse, false);
                VerifyForeignTables(typeof(T), connectionToUse, doAlter);
                string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(typeof(T));

                if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                {
                    ExecuteScalar(Creator.GetCreateForeignKeysQuery(typeof(T)), connectionToUse, false);
                }
            }
        }

        private void VerifyForeignTables(Type type, string connectionToUse, bool doAlter)
        {
            Logger.Info(string.Format("Verifying foreign tables for type {0} using connection {1}. DoAlter = {2}", type.ToString(), connectionToUse, doAlter));
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                if (!CheckIfTableExists(foreignModel.DataBaseTableName, connectionToUse))
                {
                    CreateOrAlterForeignTables(foreignModel, connectionToUse, false);
                }
            }
        }

        private Dictionary<string, ColumnDefinition> GetColumnDefinition(string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Getting Column definition for table {0} using connection {1}.", tableName, connectionToUse));
            return ((DataTable)ExecuteScalar(string.Format(QueryForColumnDefinition, tableName), connectionToUse, true)).ToDictionary<string, ColumnDefinition>(nameof(ColumnDefinition.Column_Name));
        }

        private Dictionary<string, KeyDefinition> GetKeyDefinition(string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Getting Key definition for table {0} using connection {1}.", tableName, connectionToUse));
            return ((DataTable)ExecuteScalar(string.Format(QueryForKeyDefinition, tableName), connectionToUse, true)).ToDictionary<string, KeyDefinition>(nameof(KeyDefinition.Column_Name));
        }

        private void CreateOrAlterForeignTables(IManageable foreignModel, string connectionToUse, bool doAlter)
        {
            Logger.Info(string.Format("Create or Alter foreign tables of {0} using connection {1}. DoAlter = {2}", foreignModel.DataBaseTableName, connectionToUse, doAlter));
            if (doAlter)
            {
                ExecuteScalar(Creator.GetAlterTableQuery(foreignModel.GetType(),
                                                         GetColumnDefinition(foreignModel.DataBaseTableName, connectionToUse),
                                                         GetKeyDefinition(foreignModel.DataBaseTableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(Creator.GetCreateTableQuery(foreignModel.GetType()), connectionToUse, false);
            }

            VerifyForeignTables(foreignModel.GetType(), connectionToUse, false);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(foreignModel.GetType());

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(foreignKeyQuery, connectionToUse, false);
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Checking if table {0} exists using connection {1}.", tableName, connectionToUse));
            string query = string.Format(QueryForTableExistance, TablePrefix, tableName);

            if (ExecuteScalar(query, connectionToUse, false) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected Log NewLog(string dataBaseTableName, TransactionTypes transactionType)
        {
            Log newLog = new Log
            {
                Ip = string.Empty,
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters()
            };

            Logger.Info(string.Format("Created new log object for affected table {0}, transaction used {1}, with the following parameters: {2}", newLog.DataBaseTableName, newLog.Transaccion, newLog.Parametros));

            return newLog;
        }
    }
}
