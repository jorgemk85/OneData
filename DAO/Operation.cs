using OneData.DAO.MsSql;
using OneData.DAO.MySql;
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
        public string QueryForConstraints { get; protected set; }

        protected ICreatable _creator;
        protected ConnectionTypes _connectionType;
        protected DbCommand _command;

        public Operation()
        {
            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForStoredProcedureExistance = "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_CATALOG = '{0}' AND ROUTINE_SCHEMA = '{1}' AND ROUTINE_NAME = '{2}'";
            QueryForColumnDefinition = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForConstraints = "SELECT tableConstraint.CONSTRAINT_CATALOG, tableConstraint.CONSTRAINT_SCHEMA, tableConstraint.CONSTRAINT_NAME, CONSTRAINT_TYPE, tableConstraint.TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraint INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE keyColumnUsage ON keyColumnUsage.CONSTRAINT_NAME = tableConstraint.CONSTRAINT_NAME WHERE tableConstraint.CONSTRAINT_CATALOG = '{0}' AND tableConstraint.CONSTRAINT_SCHEMA = '{1}' AND tableConstraint.TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
        }

        internal object ExecuteScalar(string transaction, string connectionToUse, bool returnDataTable)
        {
            if (string.IsNullOrWhiteSpace(transaction))
            {
                return null;
            }

            try
            {
                Logger.Info($"Starting execution for transaction using connection {connectionToUse}");
                using (DbConnection connection = _connectionType == ConnectionTypes.MySQL ? (DbConnection)Connection.OpenMySqlConnection(connectionToUse) : (DbConnection)Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    _command = connection.CreateCommand();
                    _command.CommandType = CommandType.Text;

                    if (returnDataTable)
                    {
                        _command.CommandText = transaction;
                        System.Data.DataTable dataTable = new System.Data.DataTable();
                        dataTable.Load(_command.ExecuteReader());
                        Logger.Info($"Execution for transaction using connection {connectionToUse} has finished successfully.");
                        return dataTable;
                    }
                    else
                    {
                        object scalar = null;
                        string[] queries = transaction.Split(new string[] { "|;|" }, StringSplitOptions.None);
                        for (int i = 0; i < queries.Length; i++)
                        {
                            _command.CommandText = queries[i];
                            scalar = _command.ExecuteScalar();
                        }

                        Logger.Info($"Execution for transaction using connection {connectionToUse} has finished successfully.");
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
                    throw new NotSupportedException($"El tipo de coneccion {connectionType.ToString()} no puede ser utilizado con la funcion {nameof(GetOperationBasedOnConnectionType)}.");
            }
        }

        protected string GetTransactionTextForProcedure<T>(TransactionTypes transactionType, bool doAlter) where T : Cope<T>, IManageable, new()
        {
            Logger.Info($"Getting {transactionType.ToString()} transaction for type {typeof(T)}. DoAlter = {doAlter}");
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
            Logger.Info("Getting string parameters");

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
            Logger.Info("Setting parameters in command.");
            for (int i = 0; i < parameters.Length; i++)
            {
                _command.Parameters.Add(CreateDbParameter(parameters[i].Name, parameters[i].Value));
            }
        }

        protected void SetParameters<T>(T obj, TransactionTypes transactionType, bool considerPrimary, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            Logger.Info($"Setting parameters in command based on type {typeof(T)} for transaction type {transactionType.ToString()}.");

            if (transactionType == TransactionTypes.Delete)
            {
                _command.Parameters.Add(CreateDbParameter(string.Format("_{0}", Cope<T>.ModelComposition.PrimaryKeyProperty.Name), Cope<T>.ModelComposition.PrimaryKeyProperty.GetValue(obj)));
                return;
            }

            foreach (KeyValuePair<string, PropertyInfo> property in Cope<T>.ModelComposition.FilteredProperties)
            {
                // Si la llave es primaria y es identity (autoincrement) entonces no la debe agregar como parametro.
                if (property.Value.Equals(Cope<T>.ModelComposition.PrimaryKeyProperty) && !considerPrimary)
                {
                    if (Cope<T>.ModelComposition.PrimaryKeyAttribute.IsAutoIncrement && !considerPrimary)
                    {
                        continue;
                    }
                }
                _command.Parameters.Add(CreateDbParameter("_" + property.Value.Name, property.Value.GetValue(obj)));
            }
        }

        protected void SetMassiveOperationParameters<T>(IEnumerable<T> obj, TransactionTypes transactionType, QueryOptions queryOptions) where T : Cope<T>, IManageable, new()
        {
            Logger.Info($"Setting parameters in command based massive operation transaction type {transactionType.ToString()}.");

            MassiveOperationParameter parameters = DataSerializer.GenerateCompatibleMassiveOperationXML(obj, transactionType);

            _command.Parameters.Add(CreateDbParameter("_xmlValues", parameters.XmlValues));
            _command.Parameters.Add(CreateDbParameter("_xmlNames", parameters.XmlNames));
            _command.Parameters.Add(CreateDbParameter("_procedureName", parameters.ProcedureName));
        }

        protected void PerformFullTableCheck(IManageable model, string connectionToUse)
        {
            if (model.IsFullyValidated)
            {
                Logger.Info($"Table {model.Configuration.TableName} has already been validated with it's current state.");
                return;
            }

            Logger.Info($"Processing table {model.Configuration.TableName} using connection {connectionToUse}.");
            model.IsFullyValidated = true;
            string schema = Manager.ConnectionType == ConnectionTypes.MSSQL ? model.Configuration.Schema : ConsolidationTools.GetInitialCatalog(connectionToUse, true);
            string initialCatalog = ConsolidationTools.GetInitialCatalog(connectionToUse);
            Dictionary<string, ConstraintDefinition> constraints = GetConstraints(initialCatalog, schema, model.Configuration.TableName, connectionToUse);

            if (DoTableExist(initialCatalog, schema, model.Configuration.TableName, connectionToUse))
            {
                ExecuteScalar(_creator.CreateQueryForTableAlteration(model, GetColumnDefinition(initialCatalog, model.Configuration.Schema, model.Configuration.TableName, connectionToUse), constraints), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(_creator.CreateQueryForTableCreation(model), connectionToUse, false);
            }

            VerifyForeignTables(model, connectionToUse);
            string foreignKeyQuery = _creator.GetCreateForeignKeysQuery(model, constraints);

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(foreignKeyQuery, connectionToUse, false);
            }
        }

        private void VerifyForeignTables(IManageable model, string connectionToUse)
        {
            Logger.Info($"Verifying foreign tables for type {model.GetType().ToString()} using connection {connectionToUse}.");

            foreach (KeyValuePair<string, PropertyInfo> property in model.Configuration.ForeignKeyProperties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(model.Configuration.ForeignKeyAttributes[property.Value.Name].Model);
                string schema = Manager.ConnectionType == ConnectionTypes.MSSQL ? foreignModel.Configuration.Schema : ConsolidationTools.GetInitialCatalog(connectionToUse, true);
                PerformFullTableCheck(foreignModel, connectionToUse);
            }
        }

        private Dictionary<string, ColumnDefinition> GetColumnDefinition(string initialCatalog, string schema, string tableName, string connectionToUse)
        {
            Logger.Info($"Getting Column definition for table {tableName} using connection {connectionToUse}.");
            return ((System.Data.DataTable)ExecuteScalar(string.Format(QueryForColumnDefinition, initialCatalog, schema, tableName), connectionToUse, true)).ToDictionary<string, ColumnDefinition>(nameof(ColumnDefinition.Column_Name));
        }

        private Dictionary<string, ConstraintDefinition> GetConstraints(string initialCatalog, string schema, string tableName, string connectionToUse)
        {
            Logger.Info($"Getting Constraints definition for table {tableName} using connection {connectionToUse}.");
            return ((System.Data.DataTable)ExecuteScalar(string.Format(QueryForConstraints, initialCatalog, schema, tableName), connectionToUse, true)).ToDictionary<string, ConstraintDefinition>(nameof(ConstraintDefinition.Constraint_Name));
        }

        private bool DoTableExist(string initialCatalog, string schema, string tableName, string connectionToUse)
        {
            Logger.Info($"Checking if table {tableName} exists using connection {connectionToUse}.");
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
            Logger.Info($"Checking if stored procedure {storedProcedureName} exists using connection {connectionToUse}.");
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

            Logger.Info($"Created new log object for affected table {Cope<Log>.ModelComposition.TableName}, transaction used {newLog.Transaccion}, with the following parameters: {newLog.Parametros}");

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
