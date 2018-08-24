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

        public void Initialize<T, TKey>(T mainObj) where T : IManageable<TKey>
        {
            Reset<T, TKey>(mainObj);
        }

        public void Reset<T, TKey>(T mainObj) where T : IManageable<TKey>
        {
            Cache = null;
            IsPartialCache = false;
            IsCacheEnabled = mainObj.IsCacheEnabled;
            CacheExpiration = long.Parse(mainObj.CacheExpiration.ToString()) * TimeSpan.TicksPerSecond;
            LastCacheUpdate = DateTime.Now.Ticks;
        }
    }
}
