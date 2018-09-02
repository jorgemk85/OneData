namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo DELETE en la base de datos.
    /// </summary>
    public delegate void DeleteExecutedEventHandler<T>(DeleteExecutedEventArgs<T> e);

    public class DeleteExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public DeleteExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
