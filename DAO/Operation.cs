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

        protected string GetTransactionTextForProcedure<T>(TransactionTypes transactionType, bool doAlter) where T : Cope<T>, IManageable, new()
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
                case TransactionTypes.InsertMassive:
                    return Creator.CreateInsertMassiveStoredProcedure<T>(doAlter);
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
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Manager.SelectSuffix;
                case TransactionTypes.Delete:
                    return Manager.DeleteSuffix;
                case TransactionTypes.Insert:
                    return Manager.InsertSuffix;
                case TransactionTypes.InsertMassive:
                    return Manager.InsertMassiveSuffix;
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

        protected void SetParameters<T>(T obj, TransactionTypes transactionType, bool considerPrimary) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));

            if (transactionType == TransactionTypes.Delete)
            {
                Command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Cope<T>.ModelComposition.PrimaryKeyProperty.Name), Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)));
                return;
            }

            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.FilteredProperties)
            {
                if (property.Value.Equals(Cope<T>.ModelComposition.PrimaryKeyProperty) && property.Value.PropertyType.Equals(typeof(int?)) && !considerPrimary)
                {
                    continue;
                }
                Command.Parameters.Add(CreateDbParameter("_" + property.Value.Name, property.Value.GetValue(obj)));
            }
        }

        protected void SetParameters<T>(IEnumerable<T> obj, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));

            if (transactionType == TransactionTypes.Delete)
            {
                Command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Cope<T>.ModelComposition.PrimaryKeyProperty.Name), Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)));
                return;
            }

            foreach (KeyValuePair<string, PropertyInfo> propertyInfo in Cope<T>.ModelComposition.FilteredProperties)
            {
                Command.Parameters.Add(CreateDbParameter("_" + propertyInfo.Value.Name, propertyInfo.Value.GetValue(obj)));
            }
        }

        protected void PerformTableConsolidation<T>(string connectionToUse, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Starting table consolidation for table {0} using connection {1}. DoAlter = {2}", Cope<T>.ModelComposition.TableName, connectionToUse, doAlter));
            if (!doAlter)
            {
                if (!CheckIfTableExists(Cope<T>.ModelComposition.TableName, connectionToUse))
                {
                    ProcessTable<T>(connectionToUse, false);
                    return;
                }
            }
            ExecuteScalar(Creator.CreateQueryForTableAlteration<T>(GetColumnDefinition(Cope<T>.ModelComposition.TableName, connectionToUse), GetKeyDefinition(Cope<T>.ModelComposition.TableName, connectionToUse)), connectionToUse, false);
        }

        protected void ProcessTable<T>(string connectionToUse, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Processing table {0} using connection {1}. DoAlter = {2}", Cope<T>.ModelComposition.TableName, connectionToUse, doAlter));
            if (doAlter)
            {
                PerformTableConsolidation<T>(connectionToUse, doAlter);
            }
            else
            {
                ExecuteScalar(Creator.CreateQueryForTableCreation<T>(), connectionToUse, false);
                VerifyForeignTables<T>(connectionToUse, doAlter);
                string foreignKeyQuery = Creator.GetCreateForeignKeysQuery<T>();

                if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                {
                    ExecuteScalar(foreignKeyQuery, connectionToUse, false);
                }
            }
        }

        private void VerifyForeignTables<T>(string connectionToUse, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Verifying foreign tables for type {0} using connection {1}. DoAlter = {2}", typeof(T).ToString(), connectionToUse, doAlter));

            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.ForeignKeyProperties)
            {
                IManageable foreignKey = (IManageable)Activator.CreateInstance(Cope<T>.ModelComposition.ForeignKeyAttributes[property.Value.Name].Model);
                if (!CheckIfTableExists(foreignKey.Configuration.TableName, connectionToUse))
                {
                    CreateOrAlterForeignTables<T>(foreignKey, connectionToUse, false);
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

        private void CreateOrAlterForeignTables<T>(IManageable foreignKey, string connectionToUse, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Create or Alter foreign tables of {0} using connection {1}. DoAlter = {2}", foreignKey.Configuration.TableName, connectionToUse, doAlter));
            if (doAlter)
            {
                ExecuteScalar(Creator.CreateQueryForTableAlteration<T>(
                                                         GetColumnDefinition(foreignKey.Configuration.TableName, connectionToUse),
                                                         GetKeyDefinition(foreignKey.Configuration.TableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(Creator.CreateQueryForTableCreation<T>(), connectionToUse, false);
            }

            VerifyForeignTables<T>(connectionToUse, false);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery<T>();

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
            dynamic identityId = string.Empty;

            if (Manager.Identity != null)
            {
                identityId = Manager.Identity.Configuration.PrimaryKeyProperty.GetValue(Manager.Identity);
            }

            Log newLog = new Log
            {
                Id = Guid.NewGuid(),
                IdentityId = identityId,
                Transaccion = transactionType.ToString(),
                TablaAfectada = TableName,
                Parametros = GetStringParameters()
            };

            Logger.Info(string.Format("Created new log object for affected table {0}, transaction used {1}, with the following parameters: {2}", Cope<Log>.ModelComposition.TableName, newLog.Transaccion, newLog.Parametros));

            return newLog;
        }
    }
}
