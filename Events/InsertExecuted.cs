namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT en la base de datos.
    /// </summary>
    public delegate void InsertExecutedEventHandler<T>(InsertExecutedEventArgs<T> e);

    public class InsertExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public InsertExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
