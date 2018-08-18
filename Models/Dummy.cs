using DataManagement.Interfaces;
using System;

namespace DataManagement.Models
{
    internal sealed class Dummy : IManageable
    {
        public Guid? Id { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string DataBaseTableName { get; }
        public string Schema { get; }
        public bool IsCacheEnabled { get; }
        public int CacheExpiration { get; }
    }
}
