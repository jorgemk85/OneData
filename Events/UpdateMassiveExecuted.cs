namespace OneData.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo UPDATE MASSIVE en la base de datos.
    /// </summary>
    public delegate void UpdateMassiveExecutedEventHandler<T>(UpdateMassiveExecutedEventArgs<T> e);

    public class UpdateMassiveExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public UpdateMassiveExecutedEventArgs(string tableName, Models.Result<T> result) : base(tableName, result) { }
    }
}
