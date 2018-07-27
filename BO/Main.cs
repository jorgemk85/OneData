using System;

namespace DataAccess.BO
{
    public abstract class Main
    {
        #region Properties
        public Guid? Id { get; set; }
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; } = DateTime.Now;
        #endregion

        #region Unlinked Properties
        [UnlinkedProperty]
        public string DataBaseTableName { get; }
        [UnlinkedProperty]
        public string Schema { get; }
        [UnlinkedProperty]
        public bool IsCacheEnabled { get; }
        [UnlinkedProperty]
        public int CacheExpiration { get; }
        #endregion

        #region Constructor
        public Main(Guid id, string dbTableName)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = "dbo";
            IsCacheEnabled = false;
            CacheExpiration = 0;
        }

        public Main(Guid id, string dbTableName, string schema)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
            IsCacheEnabled = false;
            CacheExpiration = 0;
        }

        public Main(Guid id, string dbTableName, bool isCacheEnabled, int cacheExpiration)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = "dbo";
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration;
        }

        public Main(Guid id, string dbTableName, string schema, bool isCacheEnabled, int cacheExpiration)
        {
            Id = id;
            DataBaseTableName = dbTableName;
            Schema = schema;
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration;
        }
        #endregion
    }
}
