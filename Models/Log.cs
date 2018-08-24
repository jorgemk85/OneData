using DataManagement.Standard.Attributes;
using System;

namespace DataManagement.Standard.Models
{
    public class Log : Main
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public Log() : base(Guid.NewGuid(), "logs") { }

        public Log(Guid id) : base(id, "logs") { }
    }
}
