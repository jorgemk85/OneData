using DataManagement.Models;
using System;

namespace DataManagement.Events
{
    public class ExecutedEventArgs<T, TKey> : EventArgs
    {
        public Result<T, TKey> Result { get; set; }
        public string TableName { get; set; }

        public ExecutedEventArgs(string tableName, Result<T, TKey> result)
        {
            TableName = tableName;
            Result = result;
        }
    }
}
