using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado cuando se haya abierto una conexion exitosa a una base de datos.
    /// </summary>
    public delegate void ConnectionOpenedEventHandler(Object sender, ConnectionOpenedEventArgs e);

    public class ConnectionOpenedEventArgs : EventArgs
    {
        public string ConnectionString { get; set; }

        public ConnectionOpenedEventArgs(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
