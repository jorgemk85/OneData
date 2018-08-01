using System;

namespace DataManagement.BO
{
    internal class Log : Main
    {
        public Guid? IdentificadorId { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public Log() : base(Guid.NewGuid(), "logs") { }

        public Log(Guid id) : base(id, "logs") { }
    }
}
