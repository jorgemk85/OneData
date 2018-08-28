namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT MASSIVE en la base de datos.
    /// </summary>
    public delegate void InsertMassiveExecutedEventHandler(InsertMassiveExecutedEventArgs e);

    public class InsertMassiveExecutedEventArgs : ExecutedEventArgs
    {
        public InsertMassiveExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
