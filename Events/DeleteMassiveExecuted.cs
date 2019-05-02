namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo DELETE MASSIVE en la base de datos.
    /// </summary>
    public delegate void DeleteMassiveExecutedEventHandler<T>(DeleteMassiveExecutedEventArgs<T> e);

    public class DeleteMassiveExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public DeleteMassiveExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
