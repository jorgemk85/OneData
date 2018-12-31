using System;

namespace DataManagement.Models
{
    internal class DataCache<T>
    {
        public bool IsEnabled { get; set; }
        public Result<T> Cache { get; set; }
        public bool IsPartialCache { get; set; } = true;
        public long LastCacheUpdate { get; set; }

        public void Initialize(bool isEnabled)
        {
            Reset(isEnabled);
        }

        public void Reset(bool isEnabled)
        {
            IsEnabled = isEnabled;
            Cache = null;
            IsPartialCache = true;
            LastCacheUpdate = DateTime.Now.Ticks;
        }
    }
}
