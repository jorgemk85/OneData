namespace DataManagement.Standard.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT LIST en la base de datos.
    /// </summary>
    public delegate void InsertListExecutedEventHandler(InsertListExecutedEventArgs e);

    public class InsertListExecutedEventArgs : ExecutedEventArgs
    {
        public InsertListExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
