using OneData.Attributes;
using OneData.Enums;
using OneData.Exceptions;
using OneData.Interfaces;
using OneData.Models;
using OneData.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OneData.DAO.MsSql
{
    internal class MsSqlOperation : Operation, IOperable
    {
        const int ERR_STORED_PROCEDURE_NOT_FOUND = 2812;
        const int ERR_OBJECT_NOT_FOUND = 208;
        const int ERR_INCORRECT_NUMBER_OF_ARGUMENTS = 8144;
        const int ERR_NOT_A_PARAMETER_FOR_PROCEDURE = 8145;
        const int ERR_EXPECTED_PARAMETER_NOT_SUPPLIED = 201;
        const int ERR_CANNOT_INSERT_EXPLICIT_VALUE_FOR_IDENTITY = 544;
        const int ERR_CANNOT_UPDATE_IDENTITY_VALUE = 8102;
        const int ERR_OPERAND_TYPE_CLASH = 206;

        public MsSqlOperation() : base()
        {
            _connectionType = ConnectionTypes.MSSQL;
            _creator = new MsSqlCreation();

            QueryForTableExistance = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForStoredProcedureExistance = "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_CATALOG = '{0}' AND ROUTINE_SCHEMA = '{1}' AND ROUTINE_NAME = '{2}'";
            QueryForColumnDefinition = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = '{0}' AND TABLE_SCHEMA = '{1}' AND TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
            QueryForConstraints = "SELECT tableConstraint.CONSTRAINT_CATALOG, tableConstraint.CONSTRAINT_SCHEMA, tableConstraint.CONSTRAINT_NAME, CONSTRAINT_TYPE, tableConstraint.TABLE_NAME, COLUMN_NAME, Update_Rule, Delete_Rule  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraint INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE keyColumnUsage ON keyColumnUsage.CONSTRAINT_NAME = tableConstraint.CONSTRAINT_NAME LEFT JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS referentialConstraints ON referentialConstraints.CONSTRAINT_NAME = tableConstraint.CONSTRAINT_NAME WHERE tableConstraint.CONSTRAINT_CATALOG = '{0}' AND tableConstraint.CONSTRAINT_SCHEMA = '{1}' AND tableConstraint.TABLE_NAME = " + $"'{Manager.TablePrefix}" + "{2}'";
        }

        public DataSet ExecuteProcedure(string tableName, string storedProcedure, QueryOptions queryOptions, Parameter[] parameters, bool logTransaction = true)
        {
            DataSet dataSet = new DataSet();

            try
            {
                Logger.Info($"Starting execution of stored procedure {storedProcedure} using connection {queryOptions.ConnectionToUse}");
                using (SqlConnection connection = Connection.OpenMsSqlConnection(queryOptions.ConnectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    _command = connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = storedProcedure;
                    if (parameters != null) SetParameters(parameters);
                    var adapter = new SqlDataAdapter((SqlCommand)_command);
                    adapter.Fill(dataSet);
                }
                Logger.Info($"Execution of stored procedure {storedProcedure} using connection {queryOptions.ConnectionToUse} has finished successfully.");
            }
            catch (SqlException SqlException)
            {
                Logger.Error(SqlException);
                throw SqlException;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, queryOptions);

            return dataSet;
        }

        public Result<T> ExecuteProcedure<T>(QueryOptions queryOptions, TransactionTypes transactionType, bool logTransaction, object obj, Expression<Func<T, bool>> expression) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = null;
            bool throwIfError = false;

        Start:
            try
            {
                Logger.Info($"Starting {transactionType.ToString()} execution for object {typeof(T)} using connection {queryOptions.ConnectionToUse}");
                if (Manager.IsPreventiveModeEnabled)
                {
                    PerformFullTableCheck(new T(), queryOptions.ConnectionToUse);
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
            catch (SqlException sqlException) when (sqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if ((Manager.IsPreventiveModeEnabled || Manager.IsReactiveModeEnabled) && !throwIfError)
                {
                    Logger.Warn($"Stored Procedure for {transactionType.ToString()} not found. Creating...");
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), queryOptions.ConnectionToUse, false);
                    throwIfError = true;
                    goto Start;
                }
                Logger.Error(sqlException);
                throw;
            }
            catch (SqlException sqlException) when (sqlException.Number == ERR_OBJECT_NOT_FOUND)
            {
                if ((Manager.IsPreventiveModeEnabled || Manager.IsReactiveModeEnabled) && !throwIfError)
                {
                    Logger.Warn($"Table {Cope<T>.ModelComposition.TableName} not found. Creating...");
                    PerformFullTableCheck(new T(), queryOptions.ConnectionToUse);
                    throwIfError = true;
                    goto Start;
                }
                Logger.Error(sqlException);
                throw;
            }
            catch (SqlException sqlException) when (sqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS ||
                                                    sqlException.Number == ERR_CANNOT_INSERT_EXPLICIT_VALUE_FOR_IDENTITY ||
                                                    sqlException.Number == ERR_EXPECTED_PARAMETER_NOT_SUPPLIED ||
                                                    sqlException.Number == ERR_CANNOT_UPDATE_IDENTITY_VALUE ||
                                                    sqlException.Number == ERR_NOT_A_PARAMETER_FOR_PROCEDURE ||
                                                    sqlException.Number == ERR_OPERAND_TYPE_CLASH)
            {
                if ((Manager.IsPreventiveModeEnabled  || Manager.IsReactiveModeEnabled) && !throwIfError)
                {
                    Logger.Warn($"Incorrect number of arguments or is identity explicit value related to the {transactionType.ToString()} stored procedure. Modifying...");
                    PerformFullTableCheck(new T(), queryOptions.ConnectionToUse);

                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, true), queryOptions.ConnectionToUse, false);
                    throwIfError = true;
                    goto Start;
                }
                Logger.Error(sqlException);
                throw;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }

            if (logTransaction) LogTransaction(Cope<T>.ModelComposition.TableName, transactionType, queryOptions);

            return result;
        }

        private Result<T> ExecuteSelectAll<T>(T obj, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (SqlConnection connection = Connection.OpenMsSqlConnection(queryOptions.ConnectionToUse))
            {
                string offsetQuery = queryOptions.Offset != 0 || queryOptions.MaximumResults > -1 ? $"OFFSET {queryOptions.Offset} ROWS" : "";
                string fetchSetup = queryOptions.Offset == 0 ? "FETCH FIRST" : "FETCH NEXT";
                string limitQuery = queryOptions.MaximumResults > -1 ? $"{fetchSetup} {queryOptions.MaximumResults} ROWS ONLY" : string.Empty;

                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.Text;

                _command.CommandText = $"{GetSelectQuerySection<T>()} {GetFromQuerySection<T>()} ORDER BY [{Cope<T>.ModelComposition.DateModifiedProperty.Name}] DESC {offsetQuery} {limitQuery}";
                FillDictionaryWithReader(_command.ExecuteReader(), ref result);
            }
            return result;
        }

        private Result<T> ExecuteProcedure<T>(T obj, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (SqlConnection connection = Connection.OpenMsSqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = string.Format("{0}.{1}{2}{3}", Cope<T>.ModelComposition.Schema, Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

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

        private Result<T> ExecuteMassiveOperation<T>(IEnumerable<T> list, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            bool throwIfError = false;

        Start:
            using (SqlConnection connection = Connection.OpenMsSqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = $"{Manager.StoredProcedurePrefix}massive_operation";

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
                catch (SqlException sqlException) when (sqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
                {
                    if (Manager.IsReactiveModeEnabled && !throwIfError)
                    {
                        Logger.Warn(string.Format("Stored Procedure for {0} not found. Creating...", transactionType.ToString()));
                        PerformStoredProcedureValidation<T>(transactionType, queryOptions);
                        throwIfError = true;
                        goto Start;
                    }
                    Logger.Error(sqlException);
                    throw;
                }

            }
            return new Result<T>(new Dictionary<dynamic, T>(), false, true);
        }

        private Result<T> ExecuteSelect<T>(Expression<Func<T, bool>> expression, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (SqlConnection connection = Connection.OpenMsSqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();

                string offsetQuery = queryOptions.Offset != 0 || queryOptions.MaximumResults > -1 ? $"OFFSET {queryOptions.Offset} ROWS" : "";
                string fetchSetup = queryOptions.Offset == 0 ? "FETCH FIRST" : "FETCH NEXT";
                string limitQuery = queryOptions.MaximumResults > -1 ? $"{fetchSetup} {queryOptions.MaximumResults} ROWS ONLY" : string.Empty;

                _command = connection.CreateCommand();
                _command.CommandType = CommandType.Text;
                _command.CommandText = $"{GetSelectQuerySection<T>()} {GetFromQuerySection<T>()} WHERE {ExpressionTools.ConvertExpressionToSQL(expression)} ORDER BY {Cope<T>.ModelComposition.DateModifiedProperty.Name} DESC {offsetQuery} {limitQuery}";
                FillDictionaryWithReader(_command.ExecuteReader(), ref result);
            }
            return result;
        }

        private string GetSelectQuerySection<T>() where T : Cope<T>, IManageable, new()
        {
            StringBuilder selectBuilder = new StringBuilder();
            IManageable foreignObject;
            string foreignTableFullyQualifiedName;
            string fullyQualifiedTableName = $"{Manager.TablePrefix}{Cope<T>.ModelComposition.TableName}";

            selectBuilder.Append($"SELECT [{fullyQualifiedTableName}].*");
            if (Cope<T>.ModelComposition.ForeignDataAttributes.Count > 0)
            {
                foreach (ForeignData foreignAttribute in Cope<T>.ModelComposition.ForeignDataAttributes.Values)
                {
                    foreignObject = (IManageable)Activator.CreateInstance(foreignAttribute.JoinModel);
                    foreignTableFullyQualifiedName = $"{Manager.TablePrefix}{foreignObject.Composition.TableName}";
                    selectBuilder.Append($",[{foreignTableFullyQualifiedName}].[{foreignAttribute.ColumnName}] as [{foreignAttribute.PropertyName}]");
                }
            }

            return selectBuilder.ToString();
        }

        private string GetFromQuerySection<T>() where T : Cope<T>, IManageable, new()
        {
            StringBuilder fromBuilder = new StringBuilder();
            IManageable foreignModel;
            IManageable foreignReferenceModel;
            string foreignTableFullyQualifiedName;
            string fullyQualifiedTableName = $"{Manager.TablePrefix}{Cope<T>.ModelComposition.TableName}";

            fromBuilder.Append($" FROM [{fullyQualifiedTableName}]");
            if (Cope<T>.ModelComposition.ForeignDataAttributes.Count > 0)
            {
                foreach (ForeignData foreignAttribute in Cope<T>.ModelComposition.ForeignDataAttributes.Values.GroupBy(x => x.JoinModel).Select(y => y.First()))
                {
                    foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.JoinModel);
                    foreignReferenceModel = (IManageable)Activator.CreateInstance(foreignAttribute.ReferenceModel);
                    foreignTableFullyQualifiedName = $"{Manager.TablePrefix}{foreignModel.Composition.TableName}";
                    fromBuilder.Append($" INNER JOIN [{foreignTableFullyQualifiedName}] ON [{Manager.TablePrefix}{foreignReferenceModel.Composition.TableName}].[{foreignAttribute.ReferenceIdName}] = [{foreignTableFullyQualifiedName}].[{foreignModel.Composition.PrimaryKeyProperty.Name}]");
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
