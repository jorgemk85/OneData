using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
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

        protected object ExecuteScalar(string transaction, string connectionToUse)
        {
            try
            {
                using (DbConnection connection = ConnectionType == ConnectionTypes.MySQL ? (DbConnection)Connection.OpenMySqlConnection(connectionToUse) : (DbConnection)Connection.OpenMsSqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = connection.CreateCommand();
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = transaction;
                    return Command.ExecuteScalar();
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

        protected string GetTransactionTextForProcedure<T>(TransactionTypes transactionType) where T : IManageable, new()
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

        protected void ProcessTableCreation<T>(string connectionToUse) where T : IManageable, new()
        {
            ExecuteScalar(Creator.GetCreateTableQuery<T>(false), connectionToUse);
            VerifyForeignTables(typeof(T), connectionToUse);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(typeof(T));

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteScalar(Creator.GetCreateForeignKeysQuery(typeof(T)), connectionToUse);
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
                    ExecuteScalar(Creator.GetCreateTableQuery(foreignModel.GetType(), false), connectionToUse);
                    VerifyForeignTables(foreignModel.GetType(), connectionToUse);
                    string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(foreignModel.GetType());

                    if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                    {
                        ExecuteScalar(foreignKeyQuery, connectionToUse);
                    }
                }
            }
        }

        private bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            string query = string.Format(CheckTableExistanceQuery, TablePrefix, tableName);

            if (ExecuteScalar(query, connectionToUse) != null)
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
