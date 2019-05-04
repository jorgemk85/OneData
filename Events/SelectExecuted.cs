namespace OneData.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT en la base de datos.
    /// </summary>
    public delegate void SelectExecutedEventHandler<T>(SelectExecutedEventArgs<T> e);

    public class SelectExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public SelectExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
