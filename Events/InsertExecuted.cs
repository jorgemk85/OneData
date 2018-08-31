namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT en la base de datos.
    /// </summary>
    public delegate void InsertExecutedEventHandler<T, TKey>(InsertExecutedEventArgs<T, TKey> e);

    public class InsertExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public InsertExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
