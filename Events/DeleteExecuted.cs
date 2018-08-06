using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo DELETE en la base de datos.
    /// </summary>
    public delegate void DeleteExecutedEventHandler(DeleteExecutedEventArgs e);

    public class DeleteExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public DeleteExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
