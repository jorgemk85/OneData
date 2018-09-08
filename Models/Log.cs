using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models
{
    [DataTable("logs")]
    public class Log : Cope<Log>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }
        public dynamic IdentityId { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }
    }
}
