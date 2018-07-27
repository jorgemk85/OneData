namespace DataAccess.BO
{
    internal class DataCache
    {
        public Result Cache { get; set; }
        public bool IsPartialCache { get; set; } = false;
        public bool IsCacheEnabled { get; set; }
        public long CacheExpiration { get; set; }
        public long LastCacheUpdate { get; set; }
    }
}
