using System;

namespace DataManagement.Standard.Models.Test
{
    public class LogTest : Main<Guid>
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public LogTest() : base(Guid.NewGuid(), "LogTests") { }

        public LogTest(Guid id) : base(id, "LogTests") { }
    }
}