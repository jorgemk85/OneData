using MySql.Data.MySqlClient;
using OneData.Attributes;
using OneData.Enums;
using OneData.Exceptions;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OneData.DAO.MySql
{
    internal class MySqlOperation : Operation, IOperable
    {
        const int ERR_TABLE_NOT_FOUND = 1146;
        const int ERR_STORED_PROCEDURE_NOT_FOUND = 1305;
        const int ERR_INCORRECT_NUMBER_OF_ARGUMENTS = 1318;
        const int ERR_UNKOWN_COLUMN = 1054;
        const int ERR_NO_DEFAULT_VALUE_IN_FIELD = 1364;
        const int ERR_INCORRECT_VALUE = 1366;

        public MySqlOperation() : base()
        {
            _connectionType = ConnectionTypes.MySQL;
            _creator = new MySqlCreation();

            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForStoredProcedureExistance = "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_CATALOG = '{0}' AND ROUTINE_SCHEMA = '{1}' AND ROUTINE_NAME = '{2}'";
            QueryForColumnDefinition = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForConstraints =
                @"SELECT DISTINCT 
	                tableConstraint.CONSTRAINT_CATALOG, 
	                tableConstraint.CONSTRAINT_SCHEMA,
	                CASE
		                WHEN tableConstraint.CONSTRAINT_NAME = 'PRIMARY' THEN CONCAT('PK_', tableConstraint.CONSTRAINT_SCHEMA, '_', tableConstraint.TABLE_NAME,'_', COLUMN_NAME)
		                ELSE tableConstraint.CONSTRAINT_NAME
	                END as CONSTRAINT_NAME,
	                CONSTRAINT_TYPE, 
	                tableConstraint.TABLE_NAME, 
	                COLUMN_NAME, 
	                Update_Rule, 
	                Delete_Rule  
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraint 
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE keyColumnUsage 
                ON keyColumnUsage.CONSTRAINT_NAME = tableConstraint.CONSTRAINT_NAME 
                LEFT JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS referentialConstraints 
                ON referentialConstraints.CONSTRAINT_NAME = tableConstraint.CONSTRAINT_NAME 
                WHERE tableConstraint.CONSTRAINT_CATALOG = '{0}' AND tableConstraint.CONSTRAINT_SCHEMA = '{1}' AND tableConstraint.TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
        }

        public DataSet ExecuteProcedure(string tableName, string storedProcedure, QueryOptions queryOptions, Parameter[] parameters, bool logTransaction = true)
        {
            DataSet dataSet = new DataSet();

            try
            {
                Logger.Info($"Starting execution of stored procedure {storedProcedure} using connection {queryOptions.ConnectionToUse}.");
                using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    _command = connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = storedProcedure;

                    if (parameters != null) SetParameters(parameters);
                    var adapter = new MySqlDataAdapter((MySqlCommand)_command);
                    adapter.Fill(dataSet);
                }
                Logger.Info($"Execution of stored procedure {storedProcedure} using connection {queryOptions.ConnectionToUse} has finished successfully.");
            }
            catch (MySqlException mySqlException)
            {
                Logger.Error(mySqlException);
                throw mySqlException;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, queryOptions);

            return dataSet;
        }

        public Result<T> ExecuteProcedure<T>(QueryOptions queryOptions, TransactionTypes transactionType, bool logTransaction, object obj, Expression<Func<T, bool>> expression) where T : IManageable, new()
        {
            Result<T> result = null;
            bool throwIfError = false;

        Start:
            try
            {
                Logger.Info($"Starting {transactionType.ToString()} execution for object {typeof(T)} using connection {queryOptions.ConnectionToUse}");
                if (Manager.IsPreventiveModeEnabled)
                {
                    PerformFullModelCheck(new T(), queryOptions.ConnectionToUse);
                }

                switch (transactionType)
                {
                    case TransactionTypes.Select:
                        result = ExecuteSelect(expression, queryOptions, transactionType);
                        break;
                    case TransactionTypes.SelectAll:
                        result = ExecuteSelectAll((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.Delete:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.DeleteMassive:
                        result = ExecuteMassiveOperation((IEnumerable<T>)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.Insert:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.InsertMassive:
                        result = ExecuteMassiveOperation((IEnumerable<T>)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.Update:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.UpdateMassive:
                        result = ExecuteMassiveOperation((IEnumerable<T>)obj, queryOptions, transactionType);
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }
                Logger.Info($"Execution {transactionType.ToString()} for object {typeof(T)} using connection {queryOptions.ConnectionToUse} has finished successfully.");
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if ((Manager.IsPreventiveModeEnabled || Manager.IsReactiveModeEnabled) && !throwIfError)
                {
                    Logger.Warn($"Stored Procedure for {transactionType.ToString()} not found. Creating...");
                    ExecuteScalar(GetTransactionTextForProcedure(new T(), transactionType, false), queryOptions.ConnectionToUse, false);
                    throwIfError = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_TABLE_NOT_FOUND)
            {
                if ((Manager.IsPreventiveModeEnabled || Manager.IsReactiveModeEnabled) && !throwIfError)
                {
                    Logger.Warn($"Table {Manager<T>.Composition.TableName} not found in database. This might be because of the quer or something stored inside a stored procedure... Creating and altering stored proecedures...");
                    PerformFullModelCheck(new T(), queryOptions.ConnectionToUse);
                    throwIfError = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS || mySqlException.Number == ERR_UNKOWN_COLUMN || mySqlException.Number == ERR_UNKOWN_COLUMN || mySqlException.Number == ERR_NO_DEFAULT_VALUE_IN_FIELD || mySqlException.Number == ERR_INCORRECT_VALUE)
            {
                if ((Manager.IsPreventiveModeEnabled || Manager.IsReactiveModeEnabled) && !throwIfError)
                {
                    PerformFullModelCheck(new T(), queryOptions.ConnectionToUse);
                    ExecuteScalar(GetTransactionTextForProcedure(new T(), transactionType, true), queryOptions.ConnectionToUse, false);
                    throwIfError = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }

            if (logTransaction) LogTransaction(Manager<T>.Composition.TableName, transactionType, queryOptions);

            return result;
        }

        private Result<T> ExecuteSelectAll<T>(T obj, QueryOptions queryOptions, TransactionTypes transactionType) where T : IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                string limitQuery = queryOptions.MaximumResults > -1 ? $"LIMIT {queryOptions.MaximumResults}" : string.Empty;
                string offsetQuery = queryOptions.Offset > 0 ? $"OFFSET {queryOptions.Offset}" : string.Empty;

                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.Text;

                _command.CommandText = $"{GetSelectQuerySection<T>()} {GetFromQuerySection<T>()} ORDER BY {queryOptions.OrderBy} {queryOptions.SortOrder.ToString()} {limitQuery} {offsetQuery}";
                FillDictionaryWithReader(_command.ExecuteReader(), ref result);
            }
            return result;
        }

        private Result<T> ExecuteProcedure<T>(T obj, QueryOptions queryOptions, TransactionTypes transactionType) where T : IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, Manager<T>.Composition.TableName, GetFriendlyTransactionSuffix(transactionType));

                switch (transactionType)
                {
                    case TransactionTypes.Delete:
                        SetParameters(obj, transactionType, true, queryOptions);
                        _command.ExecuteNonQuery();
                        break;
                    case TransactionTypes.Insert:
                        SetParameters(obj, transactionType, false, queryOptions);
                        _command.ExecuteNonQuery();
                        break;
                    case TransactionTypes.Update:
                        SetParameters(obj, transactionType, true, queryOptions);
                        _command.ExecuteNonQuery();
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }
            }
            return result;
        }

        private Result<T> ExecuteMassiveOperation<T>(IEnumerable<T> list, QueryOptions queryOptions, TransactionTypes transactionType) where T : IManageable, new()
        {
            bool throwIfError = false;

        Start:
            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = $"`{Manager.StoredProcedurePrefix}massive_operation`";

                try
                {
                    switch (transactionType)
                    {
                        case TransactionTypes.InsertMassive:
                            SetMassiveOperationParameters(list, transactionType, queryOptions);
                            _command.ExecuteNonQuery();
                            break;
                        case TransactionTypes.UpdateMassive:
                            SetMassiveOperationParameters(list, transactionType, queryOptions);
                            _command.ExecuteNonQuery();
                            break;
                        case TransactionTypes.DeleteMassive:
                            SetMassiveOperationParameters(list, transactionType, queryOptions);
                            _command.ExecuteNonQuery();
                            break;
                        default:
                            throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                    }
                }
                catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
                {
                    if (Manager.IsReactiveModeEnabled && !throwIfError)
                    {
                        Logger.Warn($"Stored Procedure for {transactionType.ToString()} not found. Creating...");
                        PerformStoredProcedureValidation<T>(transactionType, queryOptions);
                        throwIfError = true;
                        goto Start;
                    }
                    Logger.Error(mySqlException);
                    throw;
                }

            }
            return new Result<T>(new Dictionary<dynamic, T>(), false, true);
        }

        private Result<T> ExecuteSelect<T>(Expression<Func<T, bool>> expression, QueryOptions queryOptions, TransactionTypes transactionType) where T : IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                string limitQuery = queryOptions.MaximumResults > -1 ? $"LIMIT {queryOptions.MaximumResults}" : string.Empty;
                string offsetQuery = queryOptions.Offset > 0 ? $"OFFSET {queryOptions.Offset}" : string.Empty;

                _command = connection.CreateCommand();
                _command.CommandType = CommandType.Text;
                _command.CommandText = $"{GetSelectQuerySection<T>()} {GetFromQuerySection<T>()} WHERE {ExpressionTools.ConvertExpressionToSQL(expression, ref _command)} ORDER BY {queryOptions.OrderBy} {queryOptions.SortOrder.ToString()} {limitQuery} {offsetQuery}";

                FillDictionaryWithReader(_command.ExecuteReader(), ref result);
            }
            return result;
        }

        private string GetSelectQuerySection<T>() where T : IManageable, new()
        {
            StringBuilder selectBuilder = new StringBuilder();
            IManageable foreignObject;
            string foreignTableFullyQualifiedName;
            string foreignJoinModelAlias;

            selectBuilder.Append($"SELECT `{Manager.TablePrefix}{Manager<T>.Composition.TableName}`.*");
            if (Manager<T>.Composition.ForeignDataAttributes.Count > 0)
            {
                foreach (ForeignData foreignAttribute in Manager<T>.Composition.ForeignDataAttributes.Values)
                {
                    foreignObject = (IManageable)Activator.CreateInstance(foreignAttribute.JoinModel);
                    foreignTableFullyQualifiedName = $"{Manager.TablePrefix}{foreignObject.GetComposition().TableName}";

                    foreignJoinModelAlias = string.IsNullOrWhiteSpace(foreignAttribute.JoinModelTableAlias) ? foreignTableFullyQualifiedName : foreignAttribute.JoinModelTableAlias;

                    selectBuilder.Append($",`{foreignJoinModelAlias}`.`{foreignAttribute.ColumnName}` as `{foreignAttribute.PropertyName}`");
                }
            }

            return selectBuilder.ToString();
        }

        private string GetFromQuerySection<T>() where T : IManageable, new()
        {
            StringBuilder fromBuilder = new StringBuilder();
            IManageable foreignModel;
            IManageable foreignReferenceModel;
            string foreignTableFullyQualifiedName;
            string foreignJoinModelAlias;
            string foreignReferenceModelAlias;

            fromBuilder.Append($" FROM `{Manager.TablePrefix}{Manager<T>.Composition.TableName}`");
            if (Manager<T>.Composition.ForeignDataAttributes.Count > 0)
            {
                foreach (ForeignData foreignAttribute in Manager<T>.Composition.ForeignDataAttributes.Values.GroupBy(x => x.JoinModel).Select(y => y.First()))
                {
                    foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.JoinModel);
                    foreignReferenceModel = (IManageable)Activator.CreateInstance(foreignAttribute.ReferenceModel);
                    foreignTableFullyQualifiedName = $"{Manager.TablePrefix}{foreignModel.GetComposition().TableName}";

                    foreignJoinModelAlias = string.IsNullOrWhiteSpace(foreignAttribute.JoinModelTableAlias) ? foreignTableFullyQualifiedName : foreignAttribute.JoinModelTableAlias;
                    foreignReferenceModelAlias = string.IsNullOrWhiteSpace(foreignAttribute.ReferenceModelTableAlias) ? $"{Manager.TablePrefix}{foreignReferenceModel.GetComposition().TableName}" : foreignAttribute.ReferenceModelTableAlias;

                    fromBuilder.Append($" {foreignAttribute.JoinClauseType.ToString()} JOIN `{foreignTableFullyQualifiedName}` AS `{foreignJoinModelAlias}` ON `{foreignReferenceModelAlias}`.`{foreignAttribute.ReferenceIdName}` = `{foreignJoinModelAlias}`.`{foreignModel.GetComposition().PrimaryKeyProperty.Name}`");
                }
            }

            return fromBuilder.ToString();
        }

        public void LogTransaction(string tableName, TransactionTypes transactionType, QueryOptions queryOptions)
        {
            if (!Manager.EnableLogInDatabase)
            {
                return;
            }

            Logger.Info("Saving log information into the database.");
            Log newLog = NewLog(tableName, transactionType);
            ExecuteProcedure<Log>(queryOptions, TransactionTypes.Insert, false, newLog, null);
        }
    }
}

