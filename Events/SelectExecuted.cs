namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT en la base de datos.
    /// </summary>
    public delegate void SelectExecutedEventHandler<T, TKey>(SelectExecutedEventArgs<T, TKey> e);

    public class SelectExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public SelectExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
