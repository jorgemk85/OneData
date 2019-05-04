using OneData.Attributes;
using OneData.Interfaces;
using System;

namespace OneData.Models
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
        [DataLength(2550)]
        public string Parametros { get; set; }
    }
}
