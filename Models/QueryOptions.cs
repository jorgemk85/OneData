using OneData.Attributes;

namespace OneData.Models
{
    public class QueryOptions
    {
        [NotParameter]
        public string ConnectionToUse { get; set; }
        public int MaximumResults { get; set; } 
        public int Offset { get; set; } 
    }
}
