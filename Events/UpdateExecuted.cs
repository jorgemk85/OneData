namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo UPDATE en la base de datos.
    /// </summary>
    public delegate void UpdateExecutedEventHandler<T, TKey>(UpdateExecutedEventArgs<T, TKey> e);

    public class UpdateExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public UpdateExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
