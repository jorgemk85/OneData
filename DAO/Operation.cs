using DataManagement.Enums;
using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System;
using System.Data.Common;

namespace DataManagement.DAO
{
    internal class Operation
    {
        protected string SelectSuffix { get; set; }
        protected string InsertSuffix { get; set; }
        protected string UpdateSuffix { get; set; }
        protected string DeleteSuffix { get; set; }
        protected string SelectAllSuffix { get; set; }
        protected string StoredProcedurePrefix { get; set; }
        protected bool AutoCreateStoredProcedures { get; set; }
        protected bool AutoCreateTables { get; set; }

        protected DbCommand command { get; set; }

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

        internal virtual Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true)
        {
            return new Result();
        }

        internal virtual Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new()
        {
            return new Result();
        }

        internal virtual int ExecuteNonQuery(string query, string connectionToUse)
        {
            return 0;
        }

        protected string GetTransactionTextForStores<T>(TransactionTypes transactionType, ConnectionTypes connectionType) where T : IManageable, new()
        {
            switch (transactionType)
            {
                case TransactionTypes.Select:
                    return Creation.CreateSelectStoredProcedure<T>(connectionType);
                case TransactionTypes.SelectAll:
                    return Creation.CreateSelectAllStoredProcedure<T>(connectionType);
                case TransactionTypes.Delete:
                    return Creation.CreateDeleteStoredProcedure<T>(connectionType);
                case TransactionTypes.Insert:
                    return Creation.CreateInsertStoredProcedure<T>(connectionType);
                case TransactionTypes.Update:
                    return Creation.CreateUpdateStoredProcedure<T>(connectionType);
                default:
                    throw new ArgumentException("El tipo de trascaccion no es valido para generar un nuevo procedimiento almacenado.");
            }
        }
    }
}
