using DataManagement.Standard.Attributes;
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

        protected string GetTransactionTextForProcedure<T, TKey>(TransactionTypes transactionType, bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
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

        protected void SetParameters<T, TKey>(T obj, TransactionTypes transactionType) where T : IManageable<TKey> where TKey : struct
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

        protected void SetParameters<T, TKey>(List<T> obj, TransactionTypes transactionType) where T : IManageable<TKey> where TKey : struct
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

        protected void PerformTableConsolidation<T, TKey>(string connectionToUse, bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            T newObj = new T();
            Logger.Info(string.Format("Starting table consolidation for table {0} using connection {1}. DoAlter = {2}", newObj.DataBaseTableName, connectionToUse, doAlter));
            if (!doAlter)
            {
                if (!CheckIfTableExists(newObj.DataBaseTableName, connectionToUse))
                {
                    ProcessTable<T, TKey>(connectionToUse, false);
                    return;
                }
            }
            ExecuteScalar(Creator.GetAlterTableQuery<TKey>(typeof(T), GetColumnDefinition(newObj.DataBaseTableName, connectionToUse), GetKeyDefinition(newObj.DataBaseTableName, connectionToUse)), connectionToUse, false);
        }

        protected void ProcessTable<T, TKey>(string connectionToUse, bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            Logger.Info(string.Format("Processing table {0} using connection {1}. DoAlter = {2}", new T().DataBaseTableName, connectionToUse, doAlter));
            if (doAlter)
            {
                PerformTableConsolidation<T, TKey>(connectionToUse, doAlter);
            }
            else
            {
                ExecuteScalar(Creator.GetCreateTableQuery<TKey>(typeof(T)), connectionToUse, false);
                VerifyForeignTables<TKey>(typeof(T), connectionToUse, doAlter);
                string foreignKeyQuery = Creator.GetCreateForeignKeysQuery<TKey>(typeof(T));

                if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                {
                    ExecuteScalar(Creator.GetCreateForeignKeysQuery<TKey>(typeof(T)), connectionToUse, false);
                }
            }
        }

        private void VerifyForeignTables<TKey>(Type type, string connectionToUse, bool doAlter) where TKey : struct
        {
            Logger.Info(string.Format("Verifying foreign tables for type {0} using connection {1}. DoAlter = {2}", type.ToString(), connectionToUse, doAlter));
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable<TKey> foreignModel = (IManageable<TKey>)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
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

        private void CreateOrAlterForeignTables<TKey>(IManageable<TKey> foreignModel, string connectionToUse, bool doAlter) where TKey : struct
        {
            Logger.Info(string.Format("Create or Alter foreign tables of {0} using connection {1}. DoAlter = {2}", foreignModel.DataBaseTableName, connectionToUse, doAlter));
            if (doAlter)
            {
                ExecuteScalar(Creator.GetAlterTableQuery<TKey>(foreignModel.GetType(),
                                                         GetColumnDefinition(foreignModel.DataBaseTableName, connectionToUse),
                                                         GetKeyDefinition(foreignModel.DataBaseTableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(Creator.GetCreateTableQuery<TKey>(foreignModel.GetType()), connectionToUse, false);
            }

            VerifyForeignTables<TKey>(foreignModel.GetType(), connectionToUse, false);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery<TKey>(foreignModel.GetType());

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
