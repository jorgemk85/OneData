using System;

namespace DataManagement.Events
{
    /// <summary>
    /// Provocado despues de cerrar la conexion a una base de datos.
    /// </summary>
    public delegate void ConnectionClosedEventHandler(Object sender, ConnectionClosedEventArgs e);

    public class ConnectionClosedEventArgs : EventArgs
    {

    }
}
