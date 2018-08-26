using DataManagement.Enums;
using DataManagement.Models;
using System.Collections.Generic;

namespace DataManagement.Interfaces
{
    public interface IOperable
    {
        Result ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true);

        Result ExecuteProcedure<T, TKey>(T obj, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : Cope<T, TKey>, new() where TKey : struct; 

        Result ExecuteProcedure<T, TKey>(List<T> list, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : Cope<T, TKey>, new() where TKey : struct;

        void LogTransaction(string tableName, TransactionTypes transactionType, string connectionToUse);
    }
}
