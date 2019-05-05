namespace OneData.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT MASSIVE en la base de datos.
    /// </summary>
    public delegate void InsertMassiveExecutedEventHandler<T>(InsertMassiveExecutedEventArgs<T> e);

    public class InsertMassiveExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public InsertMassiveExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
