using System;

namespace DataAccess.BO
{
    public abstract class Main
    {
        public Guid? Id { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        [UnlinkedProperty]
        public string DataBaseTableName { get; set; }
        [UnlinkedProperty]
        public string Schema { get; set; }
        [UnlinkedProperty]
        public bool IsCacheEnabled { get; set; }
        [UnlinkedProperty]
        public int CacheExpiration { get; set; }

        protected Main(Guid id, string dbTableName)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = "dbo";
            IsCacheEnabled = false;
            CacheExpiration = 0;
        }

        protected Main(Guid id, string dbTableName, string schema)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
            IsCacheEnabled = false;
            CacheExpiration = 0;
        }

        protected Main(Guid id, string dbTableName, bool isCacheEnabled, int cacheExpiration)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = "dbo";
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration;
        }

        protected Main(Guid id, string dbTableName, string schema, bool isCacheEnabled, int cacheExpiration)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration;
        }
    }
}
