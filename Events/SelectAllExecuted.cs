namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT ALL en la base de datos.
    /// </summary>
    public delegate void SelectAllExecutedEventHandler<T, TKey>(SelectAllExecutedEventArgs<T, TKey> e);

    public class SelectAllExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public SelectAllExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
