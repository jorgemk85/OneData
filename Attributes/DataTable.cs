using System;

namespace OneData.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTable : Attribute
    {
        public string Schema { get; set; }
        public string TableName { get; set; }

        public DataTable(string tableName, string schema = null)
        {
            TableName = tableName;
            Schema = schema;
        }
    }
}
