using OneData.Models;
using System;

namespace OneData.Events
{
    public class ExecutedEventArgs<T> : EventArgs
    {
        public Result<T> Result { get; set; }
        public string TableName { get; set; }

        public ExecutedEventArgs(string tableName, Result<T> result)
        {
            TableName = tableName;
            Result = result;
        }
    }
}
