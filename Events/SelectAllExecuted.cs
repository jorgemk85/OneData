using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo SELECT ALL en la base de datos.
    /// </summary>
    public delegate void SelectAllExecutedEventHandler(SelectAllExecutedEventArgs e);

    public class SelectAllExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public SelectAllExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
