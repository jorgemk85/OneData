using DataManagement.Standard.Interfaces;
using System;

namespace DataManagement.Standard.Models
{
    internal class DataCache
    {
        public Result Cache { get; set; }
        public bool IsPartialCache { get; set; } = false;
        public bool IsCacheEnabled { get; set; }
        public long CacheExpiration { get; set; }
        public long LastCacheUpdate { get; set; }

        public void Initialize(bool isCacheEnabled, long cacheExpiration)
        {
            Reset(isCacheEnabled, cacheExpiration);
        }

        public void Reset(bool isCacheEnabled, long cacheExpiration)
        {
            Cache = null;
            IsPartialCache = false;
            IsCacheEnabled = isCacheEnabled;
            CacheExpiration = cacheExpiration * TimeSpan.TicksPerSecond;
            LastCacheUpdate = DateTime.Now.Ticks;
        }
    }
}
