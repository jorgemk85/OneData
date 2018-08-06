namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo DELETE en la base de datos.
    /// </summary>
    public delegate void DeleteExecutedEventHandler(DeleteExecutedEventArgs e);

    public class DeleteExecutedEventArgs : ExecutedEventArgs
    {
        public DeleteExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
