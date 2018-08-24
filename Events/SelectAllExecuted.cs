namespace DataManagement.Standard.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT ALL en la base de datos.
    /// </summary>
    public delegate void SelectAllExecutedEventHandler(SelectAllExecutedEventArgs e);

    public class SelectAllExecutedEventArgs : ExecutedEventArgs
    {
        public SelectAllExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
