using DataManagement.Attributes;
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

        protected ICreatable _creator;
        protected ConnectionTypes _connectionType;
        protected DbCommand _command;

        internal object ExecuteScalar(string transaction, string connectionToUse, bool returnDataTable)
        {
            if (string.IsNullOrWhiteSpace(transaction))
            {
                return null;
            }

            try
            {
                Logger.Info(string.Format("Starting execution for transaction using connection {0}", connectionToUse));
                using (DbConnection connection = _connectionType == ConnectionTypes.MySQL ? (DbConnection)Connection.OpenMySqlConnection(connectionToUse) : (DbConnection)Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    _command = connection.CreateCommand();
                    _command.CommandType = CommandType.Text;
                    _command.CommandText = transaction;
                    if (returnDataTable)
                    {
                        System.Data.DataTable dataTable = new System.Data.DataTable();
                        dataTable.Load(_command.ExecuteReader());
                        Logger.Info(string.Format("Execution for transaction using connection {0} has finished successfully.", connectionToUse));
                        return dataTable;
                    }
                    else
                    {
                        object scalar = _command.ExecuteScalar();
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
                case TransactionTypes.Delete:
                    return _creator.CreateDeleteStoredProcedure<T>(doAlter);
                case TransactionTypes.Insert:
                    return _creator.CreateInsertStoredProcedure<T>(doAlter);
                case TransactionTypes.InsertMassive:
                    return _creator.CreateInsertMassiveStoredProcedure<T>(doAlter);
                case TransactionTypes.Update:
                    return _creator.CreateUpdateStoredProcedure<T>(doAlter);
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

            foreach (DbParameter parametro in _command.Parameters)
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
                case TransactionTypes.Delete:
                    return Manager.DeleteSuffix;
                case TransactionTypes.Insert:
                    return Manager.InsertSuffix;
                case TransactionTypes.Update:
                    return Manager.UpdateSuffix;
                case TransactionTypes.SelectAll:
                    return Manager.SelectAllSuffix;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
            }
        }

        private DbParameter CreateDbParameter(string name, object value)
        {
            DbParameter dbParameter = _command.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            return dbParameter;
        }

        protected void SetParameters(Parameter[] parameters)
        {
            Logger.Info(string.Format("Setting parameters in command."));
            for (int i = 0; i < parameters.Length; i++)
            {
                _command.Parameters.Add(CreateDbParameter(parameters[i].Name, parameters[i].Value));
            }
        }

        protected void SetParameters<T>(T obj, TransactionTypes transactionType, bool considerPrimary, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));

            if (transactionType == TransactionTypes.Delete)
            {
                _command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Cope<T>.ModelComposition.PrimaryKeyProperty.Name), Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)));
                return;
            }

            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.FilteredProperties)
            {
                if (property.Value.Equals(Cope<T>.ModelComposition.PrimaryKeyProperty) && property.Value.PropertyType.Equals(typeof(int?)) && !considerPrimary)
                {
                    continue;
                }
                _command.Parameters.Add(CreateDbParameter("_" + property.Value.Name, property.Value.GetValue(obj)));
            }
        }

        protected void SetParameters<T>(IEnumerable<T> obj, TransactionTypes transactionType, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Setting parameters in command based on type {0} for transaction type {1}.", typeof(T), transactionType.ToString()));

            if (transactionType == TransactionTypes.Delete)
            {
                _command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Cope<T>.ModelComposition.PrimaryKeyProperty.Name), Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)));
                return;
            }

            foreach (KeyValuePair<string, PropertyInfo> propertyInfo in Cope<T>.ModelComposition.FilteredProperties)
            {
                _command.Parameters.Add(CreateDbParameter("_" + propertyInfo.Value.Name, propertyInfo.Value.GetValue(obj)));
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
            ExecuteScalar(_creator.CreateQueryForTableAlteration<T>(GetColumnDefinition(Cope<T>.ModelComposition.TableName, connectionToUse), GetKeyDefinition(Cope<T>.ModelComposition.TableName, connectionToUse)), connectionToUse, false);
        }

        protected ProcedureComposition GetStoredProcedureCode<T>(string connectionToUse, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Logger.Info($"Getting stored procedure code for object {Cope<T>.ModelComposition.TableName} type {transactionType} using connection {connectionToUse}.");
            string storedProcedure;
            switch (transactionType)
            {
                case TransactionTypes.SelectAll:
                    storedProcedure = $"{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.SelectAllSuffix}";
                    break;
                case TransactionTypes.Delete:
                    storedProcedure = $"{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.DeleteSuffix}";
                    break;
                case TransactionTypes.Insert:
                    storedProcedure = $"{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.InsertSuffix}";
                    break;
                case TransactionTypes.Update:
                    storedProcedure = $"{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{Manager.UpdateSuffix}";
                    break;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
            }
            return ((System.Data.DataTable)ExecuteScalar($"SHOW CREATE PROCEDURE {storedProcedure}", connectionToUse, true)).ToObject<ProcedureComposition>();
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
                ExecuteScalar(_creator.CreateQueryForTableCreation<T>(), connectionToUse, false);
                VerifyForeignTables<T>(connectionToUse, doAlter);
                string foreignKeyQuery = _creator.GetCreateForeignKeysQuery<T>();

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
            return ((System.Data.DataTable)ExecuteScalar(string.Format(QueryForColumnDefinition, tableName), connectionToUse, true)).ToDictionary<string, ColumnDefinition>(nameof(ColumnDefinition.Column_Name));
        }

        private Dictionary<string, KeyDefinition> GetKeyDefinition(string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Getting Key definition for table {0} using connection {1}.", tableName, connectionToUse));
            return ((System.Data.DataTable)ExecuteScalar(string.Format(QueryForKeyDefinition, tableName), connectionToUse, true)).ToDictionary<string, KeyDefinition>(nameof(KeyDefinition.Column_Name));
        }

        private void CreateOrAlterForeignTables<T>(IManageable foreignKey, string connectionToUse, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Create or Alter foreign tables of {0} using connection {1}. DoAlter = {2}", foreignKey.Configuration.TableName, connectionToUse, doAlter));
            if (doAlter)
            {
                ExecuteScalar(_creator.CreateQueryForTableAlteration<T>(
                                                         GetColumnDefinition(foreignKey.Configuration.TableName, connectionToUse),
                                                         GetKeyDefinition(foreignKey.Configuration.TableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(_creator.CreateQueryForTableCreation<T>(), connectionToUse, false);
            }

            VerifyForeignTables<T>(connectionToUse, false);
            string foreignKeyQuery = _creator.GetCreateForeignKeysQuery<T>();

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(foreignKeyQuery, connectionToUse, false);
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Checking if table {0} exists using connection {1}.", tableName, connectionToUse));
            string query = string.Format(QueryForTableExistance, $"{tableName}");

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
            dynamic identityId = 0;

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

        protected void FillDictionaryWithReader<T>(IDataReader reader, ref Result<T> result) where T : Cope<T>, IManageable, new()
        {
            using (reader)
            {
                IEnumerable<PropertyInfo> properties = DataSerializer.GetFilteredPropertiesBasedOnList<T>(reader);
                while (reader.Read())
                {
                    result.Data.Add(reader[Cope<T>.ModelComposition.PrimaryKeyProperty.Name], DataSerializer.ConvertReaderToObjectOfType<T>(reader, properties));
                }
            }
        }
    }
}
