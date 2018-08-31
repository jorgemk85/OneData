namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de cualquier tipo en la base de datos.
    /// </summary>
    public delegate void CommandExecutedEventHandler<T, TKey>(CommandExecutedEventArgs<T, TKey> e);

    public class CommandExecutedEventArgs<T, TKey> : ExecutedEventArgs<T, TKey>
    {
        public CommandExecutedEventArgs(string tableName, Models.Result<T, TKey> result) : base(tableName, result) { }
    }
}
