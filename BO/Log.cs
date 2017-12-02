using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.BO
{
    public class Log : Main
    {
        public Guid? IdentificadorId { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        public Log() : base(Guid.NewGuid(), "logs") { }

        public Log(Guid id) : base(id, "logs") { }
    }
}
