using DataManagement.Standard.Attributes;
using DataManagement.Standard.Enums;
using System;

namespace DataManagement.Standard.Models.Test
{
    public class LogTestGuid : Main<Guid>
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public LogTestGuid() : base(Guid.NewGuid(), "LogTestGuids", true, 60) { }

        public LogTestGuid(Guid id) : base(id, "LogTestGuids", true, 60) { }
    }

    public class LogTestInt : Main<int>
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public LogTestInt() : base(0, "LogTestInts", true, 60) { }

        public LogTestInt(int id) : base(id, "LogTestInts", true, 60) { }
    }
}