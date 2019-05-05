namespace OneData.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT ALL en la base de datos.
    /// </summary>
    public delegate void SelectAllExecutedEventHandler<T>(SelectAllExecutedEventArgs<T> e);

    public class SelectAllExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public SelectAllExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
