namespace DataManagement.Standard.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo STORED PROCEDURE en la base de datos.
    /// </summary>
    public delegate void StoredProcedureExecutedEventHandler(StoredProcedureExecutedEventArgs e);

    public class StoredProcedureExecutedEventArgs : ExecutedEventArgs
    {
        public StoredProcedureExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
