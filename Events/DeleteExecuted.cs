namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo DELETE en la base de datos.
    /// </summary>
    public delegate void DeleteExecutedEventHandler<T, TKey>(DeleteExecutedEventArgs<T, TKey> e);

    public class DeleteExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public DeleteExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
