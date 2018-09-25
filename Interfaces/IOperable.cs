using DataManagement.Enums;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace DataManagement.Interfaces
{
    public interface IOperable
    {
        DataSet ExecuteProcedure(string tableName, string storedProcedure, string connectionToUse, Parameter[] parameters, bool logTransaction = true);

        Result<T> ExecuteProcedure<T>(string connectionToUse, TransactionTypes transactionType, bool logTransaction, object obj, Expression<Func<T, bool>> expression) where T : Cope<T>, IManageable, new();

        void LogTransaction(string tableName, TransactionTypes transactionType, string connectionToUse);
    }
}
