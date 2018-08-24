namespace DataManagement.Standard.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo UPDATE en la base de datos.
    /// </summary>
    public delegate void UpdateExecutedEventHandler(UpdateExecutedEventArgs e);

    public class UpdateExecutedEventArgs : ExecutedEventArgs
    {
        public UpdateExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
