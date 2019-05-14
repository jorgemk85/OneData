namespace OneData.Models
{
    internal class FullyQualifiedTableName
    {
        public string Schema { get; set; }
        public string Table { get; set; }

        public FullyQualifiedTableName(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }
    }
}
