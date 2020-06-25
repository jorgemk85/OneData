using OneData.Enums;
using OneData.Models;
using System;
using System.Data;
using System.Linq.Expressions;

namespace OneData.Interfaces
{
    public interface IOperable
    {
        DataSet ExecuteProcedure(string tableName, string storedProcedure, QueryOptions queryOptions, Parameter[] parameters, bool logTransaction = true);

        Result<T> ExecuteProcedure<T>(QueryOptions queryOptions, TransactionTypes transactionType, bool logTransaction, object obj, Expression<Func<T, bool>> expression) where T : IManageable, new();

        void LogTransaction(string tableName, TransactionTypes transactionType, QueryOptions queryOptions);
    }
}
