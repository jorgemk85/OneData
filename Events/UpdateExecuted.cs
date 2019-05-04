namespace OneData.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo UPDATE en la base de datos.
    /// </summary>
    public delegate void UpdateExecutedEventHandler<T>(UpdateExecutedEventArgs<T> e);

    public class UpdateExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public UpdateExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
