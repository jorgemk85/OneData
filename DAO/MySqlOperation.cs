﻿using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DataManagement.DAO
{
    internal class MySqlOperation : Operation, IOperable
    {
        const int ERR_TABLE_NOT_FOUND = 1146;
        const int ERR_STORED_PROCEDURE_NOT_FOUND = 1305;
        const int ERR_INCORRECT_NUMBER_OF_ARGUMENTS = 1318;
        const int ERR_UNKOWN_COLUMN = 1054;
        const int ERR_NO_DEFAULT_VALUE_IN_FIELD = 1364;

        public MySqlOperation() : base()
        {
            _connectionType = ConnectionTypes.MySQL;
            _creator = new MySqlCreation();
            QueryForTableExistance = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{Manager.DefaultSchema}' AND TABLE_NAME = '{Manager.TablePrefix}" + "{0}'";
            QueryForColumnDefinition = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{Manager.DefaultSchema}' AND TABLE_NAME = '{Manager.TablePrefix}" + "{0}'";
            // TODO: Tiene escrito la columna Id como si todas las primary keys se llamaran asi. Esto esta asi por que antes era obligatorio, pero desde que se implemento el atributo PrimaryKey esto podria tronar...
            QueryForKeyDefinition = $"SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = '{Manager.DefaultSchema}' AND TABLE_NAME = '{Manager.TablePrefix}" + "{0}' AND COLUMN_NAME != 'Id'";
        }

        public DataSet ExecuteProcedure(string tableName, string storedProcedure, QueryOptions queryOptions, Parameter[] parameters, bool logTransaction = true)
        {
            DataSet dataSet = new DataSet();

            try
            {
                Logger.Info(string.Format("Starting execution of stored procedure {0} using connection {1}.", storedProcedure, queryOptions));
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
                Logger.Info(string.Format("Execution of stored procedure {0} using connection {1} has finished successfully.", storedProcedure, queryOptions));
            }
            catch (MySqlException mySqlException)
            {
                Logger.Error(mySqlException);
                throw mySqlException;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, queryOptions);

            return dataSet;
        }

        public Result<T> ExecuteProcedure<T>(QueryOptions queryOptions, TransactionTypes transactionType, bool logTransaction, object obj, Expression<Func<T, bool>> expression) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = null;
            bool overrideConsolidation = false;
            bool nextTryRepairStoredProcedure = false;

        Start:
            try
            {
                Logger.Info(string.Format("Starting {0} execution for object {1} using connection {2}", transactionType.ToString(), typeof(T), queryOptions));
                if (Manager.ConstantTableConsolidation && (Manager.IsDebug || Manager.OverrideOnlyInDebug) && !overrideConsolidation)
                {
                    PerformTableConsolidation<T>(queryOptions.ConnectionToUse, false);
                }

                switch (transactionType)
                {
                    case TransactionTypes.Select:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.SelectQuery:
                        result = ExecuteSelectQuery(expression, queryOptions, transactionType);
                        break;
                    case TransactionTypes.SelectAll:
                        result = ExecuteSelectAllQuery((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.Delete:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.Insert:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.InsertMassive:
                        result = ExecuteProcedure((IEnumerable<T>)obj, queryOptions, transactionType);
                        break;
                    case TransactionTypes.Update:
                        result = ExecuteProcedure((T)obj, queryOptions, transactionType);
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }
                Logger.Info(string.Format("Execution {0} for object {1} using connection {2} has finished successfully.", transactionType.ToString(), typeof(T), queryOptions));
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (Manager.AutoCreateStoredProcedures)
                {
                    Logger.Warn(string.Format("Stored Procedure for {0} not found. Creating...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, false), queryOptions.ConnectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_TABLE_NOT_FOUND)
            {
                if (Manager.AutoCreateTables)
                {
                    Logger.Warn(string.Format("Table {0} not found. Creating...", Cope<T>.ModelComposition.TableName));
                    ProcessTable<T>(queryOptions.ConnectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_INCORRECT_NUMBER_OF_ARGUMENTS || (mySqlException.Number == ERR_UNKOWN_COLUMN && nextTryRepairStoredProcedure))
            {
                nextTryRepairStoredProcedure = false;
                if (Manager.AutoAlterStoredProcedures)
                {
                    Logger.Warn(string.Format("Incorrect number of arguments or unkown column related to the {0} stored procedure. Modifying...", transactionType.ToString()));
                    ExecuteScalar(GetTransactionTextForProcedure<T>(transactionType, true), queryOptions.ConnectionToUse, false);
                    overrideConsolidation = true;
                    goto Start;
                }
                Logger.Error(mySqlException);
                throw;
            }
            catch (MySqlException mySqlException) when (mySqlException.Number == ERR_UNKOWN_COLUMN || mySqlException.Number == ERR_NO_DEFAULT_VALUE_IN_FIELD)
            {
                nextTryRepairStoredProcedure = true;
                if (Manager.AutoAlterTables)
                {
                    ProcessTable<T>(queryOptions.ConnectionToUse, true);
                    overrideConsolidation = true;
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

            if (logTransaction) LogTransaction(Cope<T>.ModelComposition.TableName, transactionType, queryOptions);

            return result;
        }

        private Result<T> ExecuteSelectAllQuery<T>(T obj, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                string limitQuery = queryOptions.MaximumResults > -1 ? $"LIMIT {queryOptions.MaximumResults}" : string.Empty;
                string offsetQuery = queryOptions.Offset > 0 ? $"OFFSET {queryOptions.Offset}" : string.Empty;

                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.Text;

                _command.CommandText = $"{GetSelectQuerySection<T>()} {GetFromQuerySection<T>()} ORDER BY {Cope<T>.ModelComposition.DateModifiedProperty.Name} DESC {limitQuery} {offsetQuery}";
                FillDictionaryWithReader(_command.ExecuteReader(), ref result);
            }
            return result;
        }

        private Result<T> ExecuteProcedure<T>(T obj, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

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

        private Result<T> ExecuteProcedure<T>(IEnumerable<T> list, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                _command = connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = string.Format("{0}{1}{2}", Manager.StoredProcedurePrefix, Cope<T>.ModelComposition.TableName, GetFriendlyTransactionSuffix(transactionType));

                switch (transactionType)
                {
                    case TransactionTypes.InsertMassive:
                        SetParameters(list, transactionType, queryOptions);
                        _command.ExecuteNonQuery();
                        break;
                    default:
                        throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
                }
            }
            return new Result<T>(new Dictionary<dynamic, T>(), false, true);
        }

        private Result<T> ExecuteSelectQuery<T>(Expression<Func<T, bool>> expression, QueryOptions queryOptions, TransactionTypes transactionType) where T : Cope<T>, IManageable, new()
        {
            Result<T> result = new Result<T>(new Dictionary<dynamic, T>(), false, true);

            using (MySqlConnection connection = Connection.OpenMySqlConnection(queryOptions.ConnectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                string limitQuery = queryOptions.MaximumResults > -1 ? $"LIMIT {queryOptions.MaximumResults}" : string.Empty;
                string offsetQuery = queryOptions.Offset > 0 ? $"OFFSET {queryOptions.Offset}" : string.Empty;

                _command = connection.CreateCommand();
                _command.CommandType = CommandType.Text;
                _command.CommandText = $"{GetSelectQuerySection<T>()} {GetFromQuerySection<T>()} WHERE {ExpressionTools.ConvertExpressionToSQL(expression)} ORDER BY {Cope<T>.ModelComposition.DateModifiedProperty.Name} DESC {limitQuery} {offsetQuery}";

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

            selectBuilder.Append($"SELECT `{fullyQualifiedTableName}`.*");
            if (Cope<T>.ModelComposition.ForeignDataAttributes.Count > 0)
            {
                foreach (ForeignData foreignAttribute in Cope<T>.ModelComposition.ForeignDataAttributes.Values)
                {
                    foreignObject = (IManageable)Activator.CreateInstance(foreignAttribute.JoinModel);
                    foreignTableFullyQualifiedName = $"{Manager.TablePrefix}{foreignObject.Configuration.TableName}";
                    selectBuilder.Append($",`{foreignTableFullyQualifiedName}`.`{foreignAttribute.ColumnName}` as `{foreignAttribute.PropertyName}`");
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

            fromBuilder.Append($" FROM `{fullyQualifiedTableName}`");
            if (Cope<T>.ModelComposition.ForeignDataAttributes.Count > 0)
            {
                foreach (ForeignData foreignAttribute in Cope<T>.ModelComposition.ForeignDataAttributes.Values.GroupBy(x => x.JoinModel).Select(y => y.First()))
                {
                    foreignModel = (IManageable)Activator.CreateInstance(foreignAttribute.JoinModel);
                    foreignReferenceModel = (IManageable)Activator.CreateInstance(foreignAttribute.ReferenceModel);
                    foreignTableFullyQualifiedName = $"{Manager.TablePrefix}{foreignModel.Configuration.TableName}";
                    fromBuilder.Append($" INNER JOIN `{foreignTableFullyQualifiedName}` ON `{Manager.TablePrefix}{foreignReferenceModel.Configuration.TableName}`.`{foreignAttribute.ReferenceIdName}` = `{foreignTableFullyQualifiedName}`.`{foreignModel.Configuration.PrimaryKeyProperty.Name}`");
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

            Logger.Info(string.Format("Saving log information into the database."));
            Log newLog = NewLog(tableName, transactionType);
            ExecuteProcedure<Log>(queryOptions, TransactionTypes.Insert, false, newLog, null);
        }
    }
}

