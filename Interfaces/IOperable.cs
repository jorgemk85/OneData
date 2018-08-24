using DataManagement.Standard.Enums;
using DataManagement.Standard.Models;
using System.Collections.Generic;

namespace DataManagement.Standard.Interfaces
{
    public interface IOperable
    {
        Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true);

        Result ExecuteProcedure<T>(T obj, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new();

        Result ExecuteProcedure<T>(List<T> list, string tableName, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : IManageable, new();

        void LogTransaction(string dataBaseTableName, TransactionTypes transactionType, string connectionToUse);
    }
}
