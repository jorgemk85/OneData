using System;

namespace DataManagement.BO
{
    internal class DataCache
    {
        public Result Cache { get; set; }
        public bool IsPartialCache { get; set; } = false;
        public bool IsCacheEnabled { get; set; }
        public long CacheExpiration { get; set; }
        public long LastCacheUpdate { get; set; }

        public void Initialize<T>(T mainObj)
        {
            Reset(mainObj);
        }

        public void Reset<T>(T mainObj)
        {
            try
            {
                Cache = null;
                IsPartialCache = false;
                IsCacheEnabled = (mainObj as Main).IsCacheEnabled;
                CacheExpiration = long.Parse((mainObj as Main).CacheExpiration.ToString()) * TimeSpan.TicksPerSecond;
                LastCacheUpdate = DateTime.Now.Ticks;
            }
            catch (Exception)
            {
                
            }
        }
    }
}
