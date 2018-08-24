using System.Data;

namespace DataManagement.Standard.Models
{
    public class Result
    {
        public DataTable Data { get; set; }
        public bool IsFromCache { get; set; }
        public bool IsSuccessful { get; set; }

        public Result(DataTable data, bool isFromCache, bool isSuccessful)
        {
            Data = data ?? new DataTable();
            IsFromCache = isFromCache;
            IsSuccessful = isSuccessful;
        }
    }
}
