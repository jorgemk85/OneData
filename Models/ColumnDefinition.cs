namespace DataManagement.Models
{
    /// <summary>
    /// Este modelo se usa solo para leer la informacion arrojada por el motor de base de datos al ejecutarle el comando DESCRIBE de una tabla.
    /// </summary>
    internal sealed class ColumnDefinition
    {
        public string Table_Catalog { get; set; }
        public string Table_Schema { get; set; }
        public string Table_Name { get; set; }
        public string Column_Name { get; set; }
        public int Ordinal_Position { get; set; }
        public object Column_Default { get; set; }
        public string Is_Nullable { get; set; }
        public string Data_Type { get; set; }
        public int? Character_Maximum_Length { get; set; }
        public int? Character_Octet_Length { get; set; }
        public int? Numeric_Precision { get; set; }
        public int? Numeric_Scale { get; set; }
        public string Character_Set_Name { get; set; }
        public string Collation_Name { get; set; }
        public string Column_Type { get; set; }
        public string Column_Key { get; set; }
        public string Extra { get; set; }
        public string Privileges { get; set; }
        public string Column_Comment { get; set; }

    }
}
