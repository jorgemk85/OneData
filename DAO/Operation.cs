using DataManagement.Standard.Enums;
using DataManagement.Standard.Exceptions;
using DataManagement.Standard.Extensions;
using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using DataManagement.Standard.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.Standard.DAO
{
    internal abstract class Operation
    {
        public string QueryForTableExistance { get; protected set; }
        public string QueryForColumnDefinition { get; protected set; }
        public string QueryForKeyDefinition { get; protected set; }
        public ICreatable Creator { get; set; }
        public ConnectionTypes ConnectionType { get; set; }
        public DbCommand Command { get; set; }

        internal object ExecuteScalar(string transaction, string connectionToUse, bool returnDataTable)
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
                        Logger.Info(string.Format("Execution for transaction using connection {0} has finished successfully.", connectionToUse));
                        return dataTable;
                    }
                    else
                    {
                        object scalar = Command.ExecuteScalar();
                        Logger.Info(string.Format("Execution for transaction using connection {0} has finished successfully.", connectionToUse));
                        return scalar;
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

        protected string GetTransactionTextForProcedure<T, TKey>(TransactionTypes transactionType, bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Getting {0} transaction for type {1}. DoAlter = {2}", transactionType.ToString(), typeof(T), doAlter));
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Creator.CreateSelectStoredProcedure<T, TKey>(doAlter);
                case TransactionTypes.SelectAll:
                    return Creator.CreateSelectAllStoredProcedure<T, TKey>(doAlter);
                case TransactionTypes.Delete:
                    return Creator.CreateDeleteStoredProcedure<T, TKey>(doAlter);
                case TransactionTypes.Insert:
                    return Creator.CreateInsertStoredProcedure<T, TKey>(doAlter);
                case TransactionTypes.InsertList:
                    return Creator.CreateInsertListStoredProcedure<T, TKey>(doAlter);
                case TransactionTypes.Update:
                    return Creator.CreateUpdateStoredProcedure<T, TKey>(doAlter);
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
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Manager.SelectSuffix;
                case TransactionTypes.Delete:
                    return Manager.DeleteSuffix;
                case TransactionTypes.Insert:
                    return Manager.InsertSuffix;
                case TransactionTypes.InsertList:
                    return Manager.InsertListSuffix;
                case TransactionTypes.Update:
                    return Manager.UpdateSuffix;
                case TransactionTypes.SelectAll:
                    return Manager.SelectAllSuffix;
                default:
                    return Manager.SelectAllSuffix;
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

        protected void SetParameters<T, TKey>(T obj, TransactionTypes transactionType, bool considerPrimary) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));

            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.FilteredProperties)
            {
                if (transactionType == TransactionTypes.Delete)
                {
                    if (property.Value.Name.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty.Name))
                    {
                        Command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Manager<T, TKey>.ModelComposition.PrimaryProperty.Name), property.Value.GetValue(obj)));
                        break;
                    }
                }
                else
                {
                    if (property.Value.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty) && property.Value.PropertyType.Equals(typeof(int?)) && !considerPrimary)
                    {
                        continue;
                    }
                    Command.Parameters.Add(CreateDbParameter("_" + property.Value.Name, property.Value.GetValue(obj)));
                }
            }
        }

        protected void SetParameters<T, TKey>(List<T> obj, TransactionTypes transactionType) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));

            foreach (KeyValuePair<string, PropertyInfo> propertyInfo in Manager<T, TKey>.ModelComposition.FilteredProperties)
            {
                if (transactionType == TransactionTypes.Delete)
                {
                    if (propertyInfo.Value.Name.Equals(Manager<T, TKey>.ModelComposition.PrimaryProperty.Name))
                    {
                        Command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Manager<T, TKey>.ModelComposition.PrimaryProperty.Name), propertyInfo.Value.GetValue(obj)));
                        break;
                    }
                }
                else
                {
                    Command.Parameters.Add(CreateDbParameter("_" + propertyInfo.Value.Name, propertyInfo.Value.GetValue(obj)));
                }
            }
        }

        protected void PerformTableConsolidation<T, TKey>(string connectionToUse, bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            T newObj = new T();
            Logger.Info(string.Format("Starting table consolidation for table {0} using connection {1}. DoAlter = {2}", Manager<T, TKey>.ModelComposition.TableName, connectionToUse, doAlter));
            if (!doAlter)
            {
                if (!CheckIfTableExists(Manager<T, TKey>.ModelComposition.TableName, connectionToUse))
                {
                    ProcessTable<T, TKey>(connectionToUse, false);
                    return;
                }
            }
            ExecuteScalar(Creator.CreateQueryForTableAlteration<T, TKey>(GetColumnDefinition(Manager<T, TKey>.ModelComposition.TableName, connectionToUse), GetKeyDefinition(Manager<T, TKey>.ModelComposition.TableName, connectionToUse)), connectionToUse, false);
        }

        protected void ProcessTable<T, TKey>(string connectionToUse, bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Processing table {0} using connection {1}. DoAlter = {2}", Manager<T, TKey>.ModelComposition.TableName, connectionToUse, doAlter));
            if (doAlter)
            {
                PerformTableConsolidation<T, TKey>(connectionToUse, doAlter);
            }
            else
            {
                ExecuteScalar(Creator.CreateQueryForTableCreation<T, TKey>(), connectionToUse, false);
                VerifyForeignTables<T, TKey>(connectionToUse, doAlter);
                string foreignKeyQuery = Creator.GetCreateForeignKeysQuery<T, TKey>();

                if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                {
                    ExecuteScalar(Creator.GetCreateForeignKeysQuery<T, TKey>(), connectionToUse, false);
                }
            }
        }

        private void VerifyForeignTables<T, TKey>(string connectionToUse, bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Verifying foreign tables for type {0} using connection {1}. DoAlter = {2}", typeof(T).ToString(), connectionToUse, doAlter));

            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.ForeignModelProperties)
            {
                Cope<T, TKey> foreignModel = (Cope<T, TKey>)Activator.CreateInstance(Manager<T, TKey>.ModelComposition.ForeignModelAttributes[property.Value.Name].Model);
                if (!CheckIfTableExists(foreignModel.ModelComposition.TableName, connectionToUse))
                {
                    CreateOrAlterForeignTables<T, TKey>(foreignModel, connectionToUse, false);
                }
            }
        }

        //public Manager<I, IKey> GetModelCompositionOfType<I, IKey>(Manager<I, IKey> type) where I : IManageable<IKey>, new() where IKey : struct
        //{

        //}

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

        private void CreateOrAlterForeignTables<T, TKey>(Cope<T, TKey> foreignModel, string connectionToUse, bool doAlter) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Create or Alter foreign tables of {0} using connection {1}. DoAlter = {2}", foreignModel.ModelComposition.TableName, connectionToUse, doAlter));
            if (doAlter)
            {
                ExecuteScalar(Creator.CreateQueryForTableAlteration<T, TKey>(
                                                         GetColumnDefinition(foreignModel.ModelComposition.TableName, connectionToUse),
                                                         GetKeyDefinition(foreignModel.ModelComposition.TableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(Creator.CreateQueryForTableCreation<T, TKey>(), connectionToUse, false);
            }

            VerifyForeignTables<T, TKey>(connectionToUse, false);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery<T, TKey>();

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(foreignKeyQuery, connectionToUse, false);
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Checking if table {0} exists using connection {1}.", tableName, connectionToUse));
            string query = string.Format(QueryForTableExistance, Manager.TablePrefix, tableName);

            if (ExecuteScalar(query, connectionToUse, false) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected Log NewLog(string TableName, TransactionTypes transactionType)
        {
            Log newLog = new Log
            {
                Ip = string.Empty,
                Transaccion = transactionType.ToString(),
                TablaAfectada = TableName,
                Parametros = GetStringParameters()
            };

            Logger.Info(string.Format("Created new log object for affected table {0}, transaction used {1}, with the following parameters: {2}", Manager<Log, Guid>.ModelComposition.TableName, newLog.Transaccion, newLog.Parametros));

            return newLog;
        }
    }
}
