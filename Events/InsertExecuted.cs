using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo UPDATE en la base de datos.
    /// </summary>
    public delegate void InsertExecutedEventHandler(Object sender, InsertExecutedEventArgs e);

    public class InsertExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public InsertExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
