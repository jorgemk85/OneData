using DataManagement.Standard.Models;
using System;

namespace DataManagement.Standard.Events
{
    public class ExecutedEventArgs : EventArgs
    {
        public Result Result { get; set; }
        public string TableName { get; set; }

        public ExecutedEventArgs(string tableName, Result result)
        {
            TableName = tableName;
            Result = result;
        }
    }
}
