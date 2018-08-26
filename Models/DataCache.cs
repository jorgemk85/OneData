using DataManagement.Standard.Interfaces;
using System;

namespace DataManagement.Standard.Models
{
    internal class DataCache
    {
        public Result Cache { get; set; }
        public bool IsPartialCache { get; set; } = false;
        public long LastCacheUpdate { get; set; }

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            Cache = null;
            IsPartialCache = false;
            LastCacheUpdate = DateTime.Now.Ticks;
        }
    }
}
