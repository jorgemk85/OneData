using DataManagement.Enums;
using DataManagement.Models;
using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de cualquier tipo en la base de datos.
    /// </summary>
    public delegate void CommandExecutedEventHandler(CommandExecutedEventArgs e);

    public class CommandExecutedEventArgs : EventArgs
    {
        public string TableName { get; set; }

        public CommandExecutedEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
}
