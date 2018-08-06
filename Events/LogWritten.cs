using DataManagement.Models;
using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado cuando se escribio en el log dentro de la base de datos.
    /// </summary>
    public delegate void LogWrittenEventHandler(Object sender, LogWrittenEventArgs e);

    public class LogWrittenEventArgs : EventArgs
    {
        public Log NewLog { get; set; }

        public LogWrittenEventArgs(Log newLog)
        {
            NewLog = newLog;
        }
    }
}
