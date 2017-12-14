using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace DataAccess.BO
{
    public class Result
    {
        public bool TuvoExito { get; set; }
        public string TituloMensaje { get; set; }
        public string Mensaje { get; set; }
        public DataTable Data { get; set; }

        public bool IsFromCache { get; set; }

        public Result(bool exito = false, DataTable data = null, MySqlException mse = null, ArgumentException ae = null, string titulo = "", string mensaje = "", bool isFromCache = false)
        {
            TuvoExito = exito;
            Data = data == null ? new DataTable() : data;
            TituloMensaje = titulo;
            Mensaje = mensaje;
            IsFromCache = isFromCache;
            ObtenerMensajeError(mse, ae);
        }

        private void ObtenerMensajeError(MySqlException mse, ArgumentException ae)
        {
            if (mse != null)
            {
                switch (mse.Number)
                {
                    case 1451: // Foreign Key violation
                        Mensaje = "No se puede borrar el registro ya que se encuentra asociado con otros datos en la aplicacion.";
                        TituloMensaje = "Imposible eliminar.";
                        break;
                    default:
                        Mensaje = mse.Message;
                        TituloMensaje = "Error en MySQL (" + mse.Number + ")";
                        break;
                }
                return;
            }

            if (ae != null)
            {
                Mensaje = ae.Message;
                TituloMensaje = "Error";
                return;
            }
        }
    }
}
