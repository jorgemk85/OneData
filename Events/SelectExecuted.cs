using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT en la base de datos.
    /// </summary>
    public delegate void SelectExecutedEventHandler(Object sender, SelectExecutedEventArgs e);

    public class SelectExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public SelectExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
