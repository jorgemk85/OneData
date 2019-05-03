namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT QUERY en la base de datos.
    /// </summary>
    public delegate void SelectQueryExecutedEventHandler<T>(SelectQueryExecutedEventArgs<T> e);

    public class SelectQueryExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public SelectQueryExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
