using DataManagement.Enums;
using DataManagement.Models;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Interfaces
{
    public interface IOperable
    {
        DataSet ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true);

        Result<T> ExecuteProcedure<T>(object obj, string connectionToUse, TransactionTypes transactionType, bool logTransaction) where T : Cope<T>, IManageable, new();

        void LogTransaction(string tableName, TransactionTypes transactionType, string connectionToUse);
    }
}
