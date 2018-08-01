using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataManagement.BO
{
    public class Result
    {
        public bool TuvoExito { get; set; }
        public string TituloMensaje { get; set; }
        public string Mensaje { get; set; }
        public DataTable Data { get; set; }
        public List<Object> ReturnParameters { get; set; }

        public bool IsFromCache { get; set; }

        public Result(bool exito = false, DataTable data = null, List<Object> returnParameters = null, MySqlException mysqle = null, SqlException mssqle = null, ArgumentException ae = null, string titulo = "", string mensaje = "", bool isFromCache = false)
        {
            TuvoExito = exito;
            Data = data == null ? new DataTable() : data;
            TituloMensaje = titulo;
            Mensaje = mensaje;
            IsFromCache = isFromCache;
            ReturnParameters = returnParameters;
            ObtenerMensajeError(mysqle, ae, mssqle);
        }

        private void ObtenerMensajeError(MySqlException mysqle, ArgumentException ae, SqlException mssqle)
        {
            if (mysqle != null )
            {
                switch (mysqle.Number)
                {
                    case 1451: // Foreign Key violation
                        Mensaje = "No se puede borrar el registro ya que se encuentra asociado con otros datos en la aplicacion.";
                        TituloMensaje = "Imposible eliminar.";
                        break;
                    default:
                        Mensaje = mysqle.Message;
                        TituloMensaje = "Error en MySQL (" + mysqle.Number + ")";
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

            if (mssqle != null)
            {
                switch (mssqle.Number)
                {
                    case 1451: // Foreign Key violation
                        Mensaje = "No se puede borrar el registro ya que se encuentra asociado con otros datos en la aplicacion.";
                        TituloMensaje = "Imposible eliminar.";
                        break;
                    default:
                        Mensaje = mssqle.Message;
                        TituloMensaje = "Error en MSSQL (" + mssqle.Number + ")";
                        break;
                }
                return;
            }
        }
    }
}
