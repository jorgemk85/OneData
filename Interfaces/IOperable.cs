using DataManagement.Enums;
using DataManagement.Models;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Interfaces
{
    public interface IOperable
    {
        DataSet ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true);

        Result<T> ExecuteProcedure<T>(T obj, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : Cope<T>, IManageable, new();

        Result<T> ExecuteProcedure<T>(IEnumerable<T> list, string connectionToUse, TransactionTypes transactionType, bool logTransaction = true) where T : Cope<T>, IManageable, new();

        void LogTransaction(string tableName, TransactionTypes transactionType, string connectionToUse);
    }
}
