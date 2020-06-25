using OneData.Attributes;
using OneData.Interfaces;
using System;

namespace OneData.Models
{
    [DataTable("logs")]
    public class Log : IManageable
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [DateCreated]
        public DateTime? DateCreated { get; set; }
        [DateModified]
        public DateTime? DateModified { get; set; }
        public dynamic IdentityId { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        [DataLength(2550)]
        public string Parametros { get; set; }

        private static ModelComposition _composition = new ModelComposition(typeof(Log));
        [UnmanagedProperty]
        public ModelComposition Composition { get { return _composition; } }

    }
}
