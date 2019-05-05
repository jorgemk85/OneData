using System.Collections.Generic;

namespace OneData.Models
{
    public class Result<T>
    {
        public Dictionary<dynamic, T> Data { get; set; }
        public bool IsFromCache { get; set; }
        public bool IsSuccessful { get; set; }

        public Result(Dictionary<dynamic, T> data, bool isFromCache, bool isSuccessful)
        {
            Data = data;
            IsFromCache = isFromCache;
            IsSuccessful = isSuccessful;
        }
    }
}
