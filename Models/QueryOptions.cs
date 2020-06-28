using OneData.Attributes;
using OneData.Enums;

namespace OneData.Models
{
    public class QueryOptions
    {
        public string ConnectionToUse { get; set; }
        public int MaximumResults { get; set; } 
        public int Offset { get; set; }
        public string OrderBy { get; set; }
        public SortOrderTypes SortOrder { get; set; }
        public bool UpdateNulls { get; set; }
    }
}
