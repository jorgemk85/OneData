namespace DataManagement.Standard.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT en la base de datos.
    /// </summary>
    public delegate void SelectExecutedEventHandler(SelectExecutedEventArgs e);

    public class SelectExecutedEventArgs : ExecutedEventArgs
    {
        public SelectExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
