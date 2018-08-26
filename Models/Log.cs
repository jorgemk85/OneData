using DataManagement.Attributes;
using System;

namespace DataManagement.Models
{
    [DataTableName("logs")]
    public class Log : Cope<Log, Guid>
    {
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }
        
        public Log() : base(Guid.NewGuid()) { }

        public Log(Guid id) : base(id) { }
    }
}
