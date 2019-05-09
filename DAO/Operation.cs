using OneData.Enums;
using OneData.Exceptions;
using OneData.Extensions;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OneData.DAO
{
    internal abstract class Operation
    {
        public string QueryForTableExistance { get; protected set; }
        public string QueryForStoredProcedureExistance { get; protected set; }
        public string QueryForColumnDefinition { get; protected set; }
        public string QueryForKeyDefinition { get; protected set; }

        protected ICreatable _creator;
        protected ConnectionTypes _connectionType;
        protected DbCommand _command;

        public Operation()
        {
            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForStoredProcedureExistance = "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_CATALOG = '{0}' AND ROUTINE_SCHEMA = '{1}' AND ROUTINE_NAME = '{2}'";
            QueryForColumnDefinition = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForKeyDefinition = "SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE COLUMN_NAME != '{0}' AND TABLE_CATALOG = '{1}' AND TABLE_SCHEMA = '{2}' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{3}'";
        }

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
                case TransactionTypes.DeleteMassive:
                    return _creator.CreateMassiveOperationStoredProcedure<T>(doAlter);
                case TransactionTypes.Insert:
                    return _creator.CreateInsertStoredProcedure<T>(doAlter);
                case TransactionTypes.InsertMassive:
                    return _creator.CreateMassiveOperationStoredProcedure<T>(doAlter);
                case TransactionTypes.Update:
                    return _creator.CreateUpdateStoredProcedure<T>(doAlter);
                case TransactionTypes.UpdateMassive:
                    return _creator.CreateMassiveOperationStoredProcedure<T>(doAlter);
                default:
                    ArgumentException argumentException = new ArgumentException("El tipo de transaccion no es valido para generar un nuevo procedimiento almacenado.");
                    Logger.Error(argumentException);
                    throw argumentException;
            }
        }

        protected void PerformStoredProcedureValidation<T>(TransactionTypes transactionType, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            TransactionTypes singleTransactionType;
            switch (transactionType)
            {
                case TransactionTypes.InsertMassive:
                    singleTransactionType = TransactionTypes.Insert;
                    break;
                case TransactionTypes.UpdateMassive:
                    singleTransactionType = TransactionTypes.Update;
                    break;
                case TransactionTypes.DeleteMassive:
                    singleTransactionType = TransactionTypes.Delete;
                    break;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
            }

            string schema = Manager.ConnectionType == ConnectionTypes.MSSQL ? Cope<T>.ModelComposition.Schema : ConsolidationTools.GetInitialCatalog(queryOptions.ConnectionToUse, true);
            if (!DoStoredProcedureExist(ConsolidationTools.GetInitialCatalog(queryOptions.ConnectionToUse), schema, $"{Manager.StoredProcedurePrefix}massive_operation", queryOptions.ConnectionToUse))
            {
                ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), queryOptions.ConnectionToUse, false);
            }

            if (!DoStoredProcedureExist(ConsolidationTools.GetInitialCatalog(queryOptions.ConnectionToUse), schema, $"{Manager.StoredProcedurePrefix}{Cope<T>.ModelComposition.TableName}{GetFriendlyTransactionSuffix(singleTransactionType)}", queryOptions.ConnectionToUse))
            {
                ExecuteScalar(GetTransactionTextForProcedure<T>(singleTransactionType, false), queryOptions.ConnectionToUse, false);
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

        protected void SetMassiveOperationParameters<T>(IEnumerable<T> obj, TransactionTypes transactionType, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Setting parameters in command based massive operation transaction type {1}.", typeof(T), transactionType.ToString()));

            MassiveOperationParameter parameters = DataSerializer.GenerateCompatibleMassiveOperationXML(obj, transactionType);

            _command.Parameters.Add(CreateDbParameter("_xmlValues", parameters.XmlValues));
            _command.Parameters.Add(CreateDbParameter("_xmlNames", parameters.XmlNames));
            _command.Parameters.Add(CreateDbParameter("_procedureName", parameters.ProcedureName));
        }

        protected void PerformTableConsolidation<T>(string connectionToUse, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info(string.Format("Starting table consolidation for table {0} using connection {1}. DoAlter = {2}", Cope<T>.ModelComposition.TableName, connectionToUse, doAlter));
            if (!doAlter)
            {
                string schema = Manager.ConnectionType == ConnectionTypes.MSSQL ? Cope<T>.ModelComposition.Schema : ConsolidationTools.GetInitialCatalog(connectionToUse, true);
                if (!DoTableExists(ConsolidationTools.GetInitialCatalog(connectionToUse), schema, Cope<T>.ModelComposition.TableName, connectionToUse))
                {
                    ProcessTable<T>(connectionToUse, false);
                    return;
                }
            }
            ExecuteScalar(_creator.CreateQueryForTableAlteration(new T(), GetColumnDefinition(ConsolidationTools.GetInitialCatalog(connectionToUse), Cope<T>.ModelComposition.Schema, Cope<T>.ModelComposition.TableName, connectionToUse), GetKeyDefinition(ConsolidationTools.GetInitialCatalog(connectionToUse), Cope<T>.ModelComposition.Schema, Cope<T>.ModelComposition.TableName, connectionToUse)), connectionToUse, false);
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
                ExecuteScalar(_creator.CreateQueryForTableCreation(new T()), connectionToUse, false);
                VerifyForeignTables(new T(), connectionToUse, doAlter);
                string foreignKeyQuery = _creator.GetCreateForeignKeysQuery(new T());

                if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                {
                    ExecuteScalar(foreignKeyQuery, connectionToUse, false);
                }
            }
        }

        private void VerifyForeignTables(IManageable model, string connectionToUse, bool doAlter)
        {
            Logger.Info(string.Format("Verifying foreign tables for type {0} using connection {1}. DoAlter = {2}", model.GetType().ToString(), connectionToUse, doAlter));

            foreach (KeyValuePair<string, PropertyInfo> property in model.Configuration.ForeignKeyProperties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(model.Configuration.ForeignKeyAttributes[property.Value.Name].Model);
                string schema = Manager.ConnectionType == ConnectionTypes.MSSQL ? foreignModel.Configuration.Schema : ConsolidationTools.GetInitialCatalog(connectionToUse, true);
                if (!DoTableExists(ConsolidationTools.GetInitialCatalog(connectionToUse), schema, foreignModel.Configuration.TableName, connectionToUse))
                {
                    CreateOrAlterForeignTables(foreignModel, connectionToUse, false);
                }
            }
        }

        private Dictionary<string, ColumnDefinition> GetColumnDefinition(string initialCatalog, string schema, string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Getting Column definition for table {0} using connection {1}.", tableName, connectionToUse));
            return ((System.Data.DataTable)ExecuteScalar(string.Format(QueryForColumnDefinition, initialCatalog, schema, tableName), connectionToUse, true)).ToDictionary<string, ColumnDefinition>(nameof(ColumnDefinition.Column_Name));
        }

        private Dictionary<string, KeyDefinition> GetKeyDefinition(string initialCatalog, string schema, string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Getting Key definition for table {0} using connection {1}.", tableName, connectionToUse));
            return ((System.Data.DataTable)ExecuteScalar(string.Format(QueryForKeyDefinition, initialCatalog, schema, tableName), connectionToUse, true)).ToDictionary<string, KeyDefinition>(nameof(KeyDefinition.Column_Name));
        }

        private void CreateOrAlterForeignTables(IManageable foreignModel, string connectionToUse, bool doAlter)
        {
            Logger.Info(string.Format("Create or Alter foreign tables of {0} using connection {1}. DoAlter = {2}", foreignModel.Configuration.TableName, connectionToUse, doAlter));
            string schema = Manager.ConnectionType == ConnectionTypes.MSSQL ? foreignModel.Configuration.Schema : ConsolidationTools.GetInitialCatalog(connectionToUse, true);
            if (doAlter)
            {
                ExecuteScalar(_creator.CreateQueryForTableAlteration(foreignModel,
                                                         GetColumnDefinition(ConsolidationTools.GetInitialCatalog(connectionToUse), schema, foreignModel.Configuration.TableName, connectionToUse),
                                                         GetKeyDefinition(ConsolidationTools.GetInitialCatalog(connectionToUse), schema, foreignModel.Configuration.TableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(_creator.CreateQueryForTableCreation(foreignModel), connectionToUse, false);
            }

            VerifyForeignTables(foreignModel, connectionToUse, false);
            string foreignKeyQuery = _creator.GetCreateForeignKeysQuery(foreignModel);

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(foreignKeyQuery, connectionToUse, false);
            }
        }

        private bool DoTableExists(string initialCatalog, string schema, string tableName, string connectionToUse)
        {
            Logger.Info(string.Format("Checking if table {0} exists using connection {1}.", tableName, connectionToUse));
            string query = string.Format(QueryForTableExistance, initialCatalog, schema, tableName);

            if (ExecuteScalar(query, connectionToUse, false) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool DoStoredProcedureExist(string initialCatalog, string schema, string storedProcedureName, string connectionToUse)
        {
            Logger.Info(string.Format("Checking if stored procedure {0} exists using connection {1}.", storedProcedureName, connectionToUse));
            string query = string.Format(QueryForStoredProcedureExistance, initialCatalog, schema, storedProcedureName);

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
