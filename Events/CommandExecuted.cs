namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de cualquier tipo en la base de datos.
    /// </summary>
    public delegate void CommandExecutedEventHandler<T>(CommandExecutedEventArgs<T> e);

    public class CommandExecutedEventArgs<T> : ExecutedEventArgs<T>
    {
        public CommandExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}
