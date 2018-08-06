using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo STORED PROCEDURE en la base de datos.
    /// </summary>
    public delegate void StoredProcedureExecutedEventHandler(StoredProcedureExecutedEventArgs e);

    public class StoredProcedureExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public StoredProcedureExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
