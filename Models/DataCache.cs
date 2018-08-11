using DataManagement.Interfaces;
using System;

namespace DataManagement.Models
{
    internal class DataCache
    {
        public Result Cache { get; set; }
        public bool IsPartialCache { get; set; } = false;
        public bool IsCacheEnabled { get; set; }
        public long CacheExpiration { get; set; }
        public long LastCacheUpdate { get; set; }

        public void Initialize(IManageable mainObj)
        {
            Reset(mainObj);
        }

        public void Reset(IManageable mainObj)
        {
            Cache = null;
            IsPartialCache = false;
            IsCacheEnabled = mainObj.IsCacheEnabled;
            CacheExpiration = long.Parse(mainObj.CacheExpiration.ToString()) * TimeSpan.TicksPerSecond;
            LastCacheUpdate = DateTime.Now.Ticks;
        }
    }
}
