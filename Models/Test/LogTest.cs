using DataManagement.Attributes;
using System;

namespace DataManagement.Models.Test
{
    [CacheEnabled(60)]
    public class LogTestGuid : Cope<LogTestGuid, Guid>
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public LogTestGuid() : base(Guid.NewGuid()) { }

        public LogTestGuid(Guid id) : base(id) { }
    }

    [DataTableName("LogTestInts"), CacheEnabled(60)]
    public class LogTestInt : Cope<LogTestInt, int>
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public LogTestInt() : base(0) { }

        public LogTestInt(int id) : base(id) { }
    }
}