using DataManagement.Attributes;
using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.DAO
{
    internal class MySqlOperation : IOperable
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
        public ICreatable Creator { get; set; } = new MySqlCreation();

        public MySqlOperation()
        {
            GetTransactionTypesSuffixes();
        }

        public void GetTransactionTypesSuffixes()
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

        public int ExecuteNonQuery(string transaction, string connectionToUse)
        {
            try
            {
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = new MySqlCommand(transaction, connection);
                    Command.CommandType = CommandType.Text;
                    return Command.ExecuteNonQuery();
                }
            }
            catch (MySqlException mysqle)
            {
                throw mysqle;
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }
        }

        public Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            DataTable dataTable = null;

            try
            {
                using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
                {
                    if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                    Command = new MySqlCommand(storedProcedure, connection);
                    Command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null) SetParameters(parameters);
                    dataTable = new DataTable();
                    dataTable.Load(Command.ExecuteReader());
                    dataTable.TableName = tableName;
                }
            }
            catch (MySqlException mysqle)
            {
                throw mysqle;
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }

            if (logTransaction) LogTransaction(tableName, TransactionTypes.StoredProcedure, connectionToUse);

            return new Result(dataTable);
        }

        public Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new()
        {
            DataTable dataTable = null;

            Start:
            try
            {
                dataTable = ConfigureConnectionAndExecuteCommand(obj, tableName, connectionToUse, transactionType);
            }
            catch (MySqlException mysqle) when (mysqle.Number == 2812)
            {
                if (AutoCreateStoredProcedures)
                {
                    ExecuteNonQuery(GetTransactionTextForStores<T>(transactionType), connectionToUse);
                    goto Start;
                }
                else
                {
                    throw mysqle;
                }
            }
            catch (MySqlException mysqle) when (mysqle.Number == 208)
            {
                if (AutoCreateTables)
                {
                    ProcessTableCreation<T>(connectionToUse);

                    goto Start;
                }
                else
                {
                    throw mysqle;
                }
            }

            if (logTransaction) LogTransaction(tableName, transactionType, connectionToUse);

            return new Result(dataTable);
        }

        public void ProcessTableCreation<T>(string connectionToUse) where T : IManageable, new()
        {
            ExecuteNonQuery(Creator.GetCreateTableQuery<T>(), connectionToUse);
            VerifyForeignTables(typeof(T), connectionToUse);
            string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(typeof(T));

            if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
            {
                ExecuteNonQuery(Creator.GetCreateForeignKeysQuery(typeof(T)), connectionToUse);
            }
        }

        public void VerifyForeignTables(Type type, string connectionToUse)
        {
            PropertyInfo[] properties = type.GetProperties().Where(q => q.GetCustomAttribute<UnlinkedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                IManageable foreignModel = (IManageable)Activator.CreateInstance(property.GetCustomAttribute<ForeignModel>().Model);
                if (!CheckIfTableExists(foreignModel.DataBaseTableName, connectionToUse))
                {
                    ExecuteNonQuery(Creator.GetCreateTableQuery(foreignModel.GetType()), connectionToUse);
                    VerifyForeignTables(foreignModel.GetType(), connectionToUse);
                    string foreignKeyQuery = Creator.GetCreateForeignKeysQuery(foreignModel.GetType());

                    if (!string.IsNullOrWhiteSpace(foreignKeyQuery))
                    {
                        ExecuteNonQuery(foreignKeyQuery, connectionToUse);
                    }
                }
            }
        }

        public bool CheckIfTableExists(string tableName, string connectionToUse)
        {
            string query = string.Format("SELECT name FROM sysobjects WHERE name='{0}' AND xtype='U'", tableName);

            if (ExecuteNonQuery(query, connectionToUse) <= 0)
            {
                return false;
            }
            return true;
        }

        public DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType) where T : IManageable, new()
        {
            DataTable dataTable = null;

            using (MySqlConnection connection = Connection.OpenMySqlConnection(connectionToUse))
            {
                if (connection.State != ConnectionState.Open) throw new BadConnectionStateException();
                Command = new MySqlCommand(string.Format("{0}.{1}{2}{3}", obj.Schema, StoredProcedurePrefix, tableName, GetFriendlyTransactionSuffix(transactionType)), connection);
                Command.CommandType = CommandType.StoredProcedure;

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

        public void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse)
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

        public string GetStringParameters()
        {
            StringBuilder builder = new StringBuilder();

            foreach (MySqlParameter parametro in Command.Parameters)
            {
                if (parametro.Value != null)
                {
                    builder.AppendFormat("{0}: {1}|", parametro.ParameterName, parametro.Value);
                }
            }

            return builder.ToString();
        }

        public void SetParameters<T>(T obj, TransactionTypes transactionType)
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                // Si encontramos el atributo entonces se brinca la propiedad.
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(UnlinkedProperty)) != null) continue;

                if (transactionType == TransactionTypes.Delete)
                {
                    if (propertyInfo.Name == "Id")
                    {
                        ((MySqlCommand)Command).Parameters.AddWithValue("_id", propertyInfo.GetValue(obj));
                        break;
                    }
                }
                else
                {
                    ((MySqlCommand)Command).Parameters.AddWithValue("_" + propertyInfo.Name, propertyInfo.GetValue(obj));
                }
            }
        }

        public void SetParameters(Parameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                ((MySqlCommand)Command).Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);
            }
        }

        public string GetFriendlyTransactionSuffix(TransactionTypes transactionType)
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

        public string GetTransactionTextForStores<T>(TransactionTypes transactionType) where T : IManageable, new()
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Creator.CreateSelectStoredProcedure<T>();
                case TransactionTypes.SelectAll:
                    return Creator.CreateSelectAllStoredProcedure<T>();
                case TransactionTypes.Delete:
                    return Creator.CreateDeleteStoredProcedure<T>();
                case TransactionTypes.Insert:
                    return Creator.CreateInsertStoredProcedure<T>();
                case TransactionTypes.Update:
                    return Creator.CreateUpdateStoredProcedure<T>();
                default:
                    throw new ArgumentException("El tipo de trascaccion no es valido para generar un nuevo procedimiento almacenado.");
            }
        }
    }
}
