using DataManagement.Enums;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Interfaces
{
    internal interface IOperable
    {
        string SelectSuffix { get; set; }
        string InsertSuffix { get; set; }
        string UpdateSuffix { get; set; }
        string DeleteSuffix { get; set; }
        string SelectAllSuffix { get; set; }
        string StoredProcedurePrefix { get; set; }
        bool AutoCreateStoredProcedures { get; set; }
        bool AutoCreateTables { get; set; }
        bool EnableLog { get; set; }
        DbCommand Command { get; set; }
        ICreatable Creator { get; set; }

        void GetTransactionTypesSuffixes();

        string GetFriendlyTransactionSuffix(TransactionTypes transactionType);

        Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true);

        Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new();

        int ExecuteNonQuery(string query, string connectionToUse);

        string GetTransactionTextForStores<T>(TransactionTypes transactionType) where T : IManageable, new();

        void ProcessTableCreation<T>(string connectionToUse) where T : IManageable, new();

        void VerifyForeignTables(Type type, string connectionToUse);

        bool CheckIfTableExists(string tableName, string connectionToUse);

        DataTable ConfigureConnectionAndExecuteCommand<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType) where T : IManageable, new();

        void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse);

        void SetParameters<T>(T obj, TransactionTypes transactionType);

        string GetStringParameters();

        void SetParameters(Parameter[] parameters);
    }
}
