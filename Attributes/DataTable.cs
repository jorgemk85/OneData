using System;

namespace OneData.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTable : Attribute
    {
        public string Schema { get; set; }
        public string TableName { get; set; }

        /// <summary>
        /// Use this attribute to specify what table will be used with this model.
        /// </summary>
        /// <param name="tableName">The name of your table.</param>
        /// <param name="schema">The name of your schema, if applicable. This setting is used only with Microsoft SQL.</param>
        public DataTable(string tableName, string schema = null)
        {
            TableName = tableName;
            Schema = schema;
        }
    }
}
