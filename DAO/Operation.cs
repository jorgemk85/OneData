using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal abstract class Operation
    {
        public string SelectSuffix { get; set; }
        public string InsertSuffix { get; set; }
        public string UpdateSuffix { get; set; }
        public string DeleteSuffix { get; set; }
        public string SelectAllSuffix { get; set; }
        public string StoredProcedurePrefix { get; set; }
        public bool AutoCreateStoredProcedures { get; set; }
        public bool AutoCreateTables { get; set; }
        public bool EnableLog { get; set; }
        public DbCommand Command { get; set; }
        public ICreatable Creator { get; set; }
        public ConnectionTypes ConnectionType { get; set; }

        protected const int ERR_STORED_PROCEDURE_NOT_FOUND = 2812;
        protected const int ERR_OBJECT_NOT_FOUND = 208;

        public Operation()
        {
            GetTransactionTypesSuffixes();
        }

        private void GetTransactionTypesSuffixes()
        {
            SelectSuffix = ConsolidationTools.GetValueFromConfiguration("SelectSuffix", ConfigurationTypes.AppSetting);
            InsertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            UpdateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            DeleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
            SelectAllSuffix = ConsolidationTools.GetValueFromConfiguration("SelectAllSuffix", ConfigurationTypes.AppSetting);
            StoredProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);

            AutoCreateStoredProcedures = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateStoredProcedures", ConfigurationTypes.AppSetting));
            AutoCreateTables = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateTables", ConfigurationTypes.AppSetting));
            EnableLog = bool.Parse(ConsolidationTools.GetValueFromConfiguration("EnableLog", ConfigurationTypes.AppSetting));
        }

        internal static Operation GetOperationBasedOnConnectionType(ConnectionTypes connectionType)
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

        private int ExecuteNonQuery(string transaction, string connectionToUse, ConnectionTypes connectionType)
        {
            try
            {
                using (DbConnection connection = Connection.OpenConnection(connectionToUse, connectionType))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = transaction;
                    return Command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                using (DbConnection connection = Connection.OpenConnection(connectionToUse, ConnectionType))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = storedProcedure;

                    if (parameters != null) SetParameters(parameters);
                    dataTable = new DataTable();
                    dataTable.Load(Command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (DbException dbe)
            {
                throw dbe;
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return new Result(dataTable);
        }

        internal Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new()
        {
            DataTable dataTable = null;

        Start:
            try
            {
                dataTable = ConfigureConnectionAndExecuteCommand(obj, tableName, connectionToUse, transactionType);
            }
            catch (DbException sqle) when (((SqlException)sqle).Number == ERR_STORED_PROCEDURE_NOT_FOUND)
            {
                if (AutoCreateStoredProcedures)
                {
                    ExecuteNonQuery(GetTransactionTextForStores<T>(transactionType), connectionToUse, ConnectionType);
                    goto Start;
                }
                else
                {
                    throw sqle;
                }
            }
            catch (DbException sqle) when (((SqlException)sqle).Number == ERR_OBJECT_NOT_FOUND)
            {
                if (AutoCreateTables)
                {
                    ProcessTableCreation<T>(connectionToUse);

                    goto Start;
                }
                else
                {
                    throw sqle;
                }
            }
            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable);
        }

        private DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType) where T : IManageable, new()
        {
            DataTable dataTable = null;

            using (DbConnection connection = Connection.OpenConnection(connectionToUse, ConnectionType))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                Command = connection.CreateCommand();
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = string.Format("{0}.{1}{2}{3}", obj.Schema, StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType));

                if (transactionType == TransactionTypes.Insert || transactionType == TransactionTypes.Update || transactionType == TransactionTypes.Delete)
                {
                    SetParameters(obj, transactionType);
                    Command.ExecuteNonQuery();
                }
                else
                {
                    if (transactionType == TransactionTypes.Select)
                    {
                        SetParameters(obj, transactionType);
                    }
                    dataTable = new DataTable();
                    dataTable.Load(Command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }

            return dataTable;
        }

        private string GetTransactionTextForStores<T>(TransactionTypes transactionType) where T : IManageable, new()
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Creator.CreateSelectStoredProcedure<T>(false);
                case TransactionTypes.SelectAll:
                    return Creator.CreateSelectAllStoredProcedure<T>(false);
                case TransactionTypes.Delete:
                    return Creator.CreateDeleteStoredProcedure<T>(false);
                case TransactionTypes.Insert:
                    return Creator.CreateInsertStoredProcedure<T>(false);
                case TransactionTypes.Update:
                    return Creator.CreateUpdateStoredProcedure<T>(false);
                default:
                    throw new ArgumentException("El tipo de trascaccion no es valido para generar un nuevo procedimiento almacenado.");
            }
        }

        private void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse)
        {
            if (!EnableLog)
            {
                return;
            }
            Log newLog = new Log
            {
                Ip = string.Empty,
                Transaccion = transactionType.ToString(),
                TablaAfectada = dataBaseTableName,
                Parametros = GetStringParameters()
            };

            ExecuteProcedure(newLog, newLog.DataBaseTableName, connectionToUse, TransactionTypes.Insert, false);
        }

        private string GetStringParameters()
        {
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

        private string GetFriendlyTransactionSuffix(TransactionTypes transactionType)
        {
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

        private void ProcessTableCreation<T>(string connectionToUse) where T : IManageable, new()
        {
            ExecuteNonQuery(Creator.GetCreateTableQuery<T>(false), connectionToUse, ConnectionType);
            VerifyForeignTables(typeof(T), connectionToUse);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(typeof(T));

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteNonQuery(Creator.GetCreateForeignKeysQuery(typeof(T)), connectionToUse, ConnectionType);
            }
        }

        private void VerifyForeignTables(Type type, string connectionToUse)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                if (!CheckIfTableExists(foreignModel.DataBaseTableName, connectionToUse))
                {
                    ExecuteNonQuery(Creator.GetCreateTableQuery(foreignModel.GetType(), false), connectionToUse, ConnectionType);
                    VerifyForeignTables(foreignModel.GetType(), connectionToUse);
                    string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(foreignModel.GetType());

                    if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                    {
                        ExecuteNonQuery(foreignKeyQuery, connectionToUse, ConnectionType);
                    }
                }
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            string query = string.Format("SELECT name FROM sysobjects WHERE name='{0}' AND xtype='U'", tableName);

            if (ExecuteNonQuery(query, connectionToUse, ConnectionType) <= 0)
            {
                return false;
            }
            return true;
        }

        private DbParameter CreateDbParameter(string name, object value)
        {
            DbParameter dbParameter = Command.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            return dbParameter;
        }

        private void SetParameters(Parameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                Command.Parameters.Add(CreateDbParameter(parameters[i].Name, parameters[i].Value));
            }
        }

        private void SetParameters<T>(T obj, TransactionTypes transactionType)
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo entonces se brinca la propiedad.
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(UnlinkedProperty)) != null) continue;

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
    }
}
