using System;

namespace DataManagement.Standard.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTableName : Attribute
    {
        public string Schema { get; set; }
        public string TableName { get; set; }

        public DataTableName(string tableName, string schema = null)
        {
            TableName = tableName;
            Schema = schema;
        }
    }
}
