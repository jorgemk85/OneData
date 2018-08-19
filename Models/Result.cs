using System.Data;

namespace DataManagement.Models
{
    public class Result
    {
        public DataTable Data { get; set; }

        public bool IsFromCache { get; set; }

        public Result(DataTable data = null, bool isFromCache = false)
        {
            Data = data ?? new DataTable();
            IsFromCache = isFromCache;
        }
    }
}
