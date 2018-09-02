namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo STORED PROCEDURE en la base de datos.
    /// </summary>
    public delegate void StoredProcedureExecutedEventHandler<T>(StoredProcedureExecutedEventArgs<T> e);

    public class StoredProcedureExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public StoredProcedureExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
