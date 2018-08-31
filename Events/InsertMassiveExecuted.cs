namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT MASSIVE en la base de datos.
    /// </summary>
    public delegate void InsertMassiveExecutedEventHandler<T, TKey>(InsertMassiveExecutedEventArgs<T, TKey> e);

    public class InsertMassiveExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public InsertMassiveExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
