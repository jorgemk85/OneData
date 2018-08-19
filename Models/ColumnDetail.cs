namespace DataManagement.Models
{
    /// <summary>
    /// Este modelo se usa solo para leer la informacion arrojada por el motor de base de datos al ejecutarle el comando DESCRIBE de una tabla.
    /// </summary>
    internal sealed class ColumnDetail
    {
        public string Field { get; set; }
        public string Type { get; set; }
        public string Null { get; set; }
        public string Key { get; set; }
        public object Default { get; set; }
        public string Extra { get; set; }
    }
}
