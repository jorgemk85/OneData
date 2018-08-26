using System;

namespace DataManagement.Models
{
    internal class DataCache
    {
        public bool IsEnabled { get; set; }
        public Result Cache { get; set; }
        public bool IsPartialCache { get; set; } = false;
        public long LastCacheUpdate { get; set; }

        public void Initialize(bool isEnabled)
        {
            Reset(isEnabled);
        }

        public void Reset(bool isEnabled)
        {
            IsEnabled = isEnabled;
            Cache = null;
            IsPartialCache = false;
            LastCacheUpdate = DateTime.Now.Ticks;
        }
    }
}
