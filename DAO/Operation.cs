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
        public string SelectSuffix { get; set; }
        public string InsertSuffix { get; set; }
        public string UpdateSuffix { get; set; }
        public string DeleteSuffix { get; set; }
        public string SelectAllSuffix { get; set; }
        public string StoredProcedurePrefix { get; set; }
        public string TablePrefix { get; set; }
        public bool AutoCreateStoredProcedures { get; set; }
        public bool AutoCreateTables { get; set; }
        public bool EnableLog { get; set; }
        public ICreatable Creator { get; set; }
        public ConnectionTypes ConnectionType { get; set; }
        public DbCommand Command { get; set; }
        public string CheckTableExistanceQuery { get; protected set; }

        public Operation()
        {
            GetTransactionTypesSuffixes();
        }

        protected object ExecuteScalar(string transaction, string connectionToUse, bool returnDataTable)
        {
            try
            {
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
                        return dataTable;
                    }
                    else
                    {
                        return Command.ExecuteScalar();
                    }
                }
            }
            catch (DbException dbException)
            {
                if (dbException.InnerException != null)
                {
                    if (dbException.InnerException.Message.EndsWith("must be defined."))
                    {
                        throw new AllowUserVariableNotEnabledException();
                    }
                }

                throw dbException;
            }
        }

        private void GetTransactionTypesSuffixes()
        {
            SelectSuffix = ConsolidationTools.GetValueFromConfiguration("SelectSuffix", ConfigurationTypes.AppSetting);
            InsertSuffix = ConsolidationTools.GetValueFromConfiguration("InsertSuffix", ConfigurationTypes.AppSetting);
            UpdateSuffix = ConsolidationTools.GetValueFromConfiguration("UpdateSuffix", ConfigurationTypes.AppSetting);
            DeleteSuffix = ConsolidationTools.GetValueFromConfiguration("DeleteSuffix", ConfigurationTypes.AppSetting);
            SelectAllSuffix = ConsolidationTools.GetValueFromConfiguration("SelectAllSuffix", ConfigurationTypes.AppSetting);
            StoredProcedurePrefix = ConsolidationTools.GetValueFromConfiguration("StoredProcedurePrefix", ConfigurationTypes.AppSetting);
            TablePrefix = ConsolidationTools.GetValueFromConfiguration("TablePrefix", ConfigurationTypes.AppSetting);

            AutoCreateStoredProcedures = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateStoredProcedures", ConfigurationTypes.AppSetting));
            AutoCreateTables = bool.Parse(ConsolidationTools.GetValueFromConfiguration("AutoCreateTables", ConfigurationTypes.AppSetting));
            EnableLog = bool.Parse(ConsolidationTools.GetValueFromConfiguration("EnableLog", ConfigurationTypes.AppSetting));
        }

        internal static IOperable GetOperationBasedOnConnectionType(ConnectionTypes connectionType)
        {
            switch (connectionType)
            {
                case ConnectionTypes.MySQL:
                    return new MySqlOperation();
                case ConnectionTypes.MSSQL:
                    return new MySqlOperation();
                default:
                    return new MySqlOperation();
            }
        }

        protected string GetTransactionTextForProcedure<T>(TransactionTypes transactionType, bool doAlter) where T : IManageable, new()
        {
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
                case TransactionTypes.Update:
                    return Creator.CreateUpdateStoredProcedure<T>(doAlter);
                default:
                    throw new ArgumentException("El tipo de trascaccion no es valido para generar un nuevo procedimiento almacenado.");
            }
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

        protected string GetFriendlyTransactionSuffix(TransactionTypes transactionType)
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

        private DbParameter CreateDbParameter(string name, object value)
        {
            DbParameter dbParameter = Command.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            return dbParameter;
        }

        protected void SetParameters(Parameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                Command.Parameters.Add(CreateDbParameter(parameters[i].Name, parameters[i].Value));
            }
        }

        protected void SetParameters<T>(T obj, TransactionTypes transactionType)
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

        protected void ProcessTable<T>(string connectionToUse, bool doAlter) where T : IManageable, new()
        {
            if (doAlter)
            {
                ExecuteScalar(Creator.GetAlterTableQuery(typeof(T), GetTableDefinition(new T().DataBaseTableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(Creator.GetCreateTableQuery(typeof(T)), connectionToUse, false);
                VerifyForeignTables(typeof(T), connectionToUse, doAlter);
                string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(typeof(T));

                if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                {
                    ExecuteScalar(Creator.GetCreateForeignKeysQuery(typeof(T)), connectionToUse, false);
                }
            }
        }

        private void VerifyForeignTables(Type type, string connectionToUse, bool doAlter)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                if (!CheckIfTableExists(foreignModel.DataBaseTableName, connectionToUse))
                {
                    CreateOrAlterForeignTables(foreignModel, connectionToUse, false);
                }
            }
        }

        private Dictionary<string, ColumnDetail> GetTableDefinition(string tableName, string connectionToUse)
        {
            return ((DataTable)ExecuteScalar(string.Format("DESCRIBE {0}", tableName), connectionToUse, true)).ToDictionary<string, ColumnDetail>("Field");
        }

        private void CreateOrAlterForeignTables(IManageable foreignModel, string connectionToUse, bool doAlter)
        {
            if (doAlter)
            {
                ExecuteScalar(Creator.GetAlterTableQuery(foreignModel.GetType(), GetTableDefinition(foreignModel.DataBaseTableName, connectionToUse)), connectionToUse, false);
            }
            else
            {
                ExecuteScalar(Creator.GetCreateTableQuery(foreignModel.GetType()), connectionToUse, false);
            }
            
            VerifyForeignTables(foreignModel.GetType(), connectionToUse, false);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(foreignModel.GetType());

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(foreignKeyQuery, connectionToUse, false);
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            string query = string.Format(CheckTableExistanceQuery, TablePrefix, tableName);

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

            return newLog;
        }
    }
}
