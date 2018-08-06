using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo UPDATE en la base de datos.
    /// </summary>
    public delegate void UpdateExecutedEventHandler(UpdateExecutedEventArgs e);

    public class UpdateExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public UpdateExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
