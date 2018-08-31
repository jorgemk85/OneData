namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo STORED PROCEDURE en la base de datos.
    /// </summary>
    public delegate void StoredProcedureExecutedEventHandler<T, TKey>(StoredProcedureExecutedEventArgs<T, TKey> e);

    public class StoredProcedureExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public StoredProcedureExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
