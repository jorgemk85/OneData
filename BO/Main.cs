using System;

namespace DataAccess.BO
{
    public abstract class Main
    {
        public Guid? Id { get; set; }
        [UnlinkedProperty]
        public string DataBaseTableName { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [UnlinkedProperty]
        public string Schema { get; set; }

        protected Main(Guid id, string dbTableName)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = "dbo";
        }

        protected Main(Guid id, string dbTableName, string schema)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
        }
    }
}
