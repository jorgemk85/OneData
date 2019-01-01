using DataManagement.Attributes;

namespace DataManagement.Models
{
    public class QueryOptions
    {
        [NotParameter]
        public string ConnectionToUse { get; set; }
        public int MaximumResults { get; set; } 
        public int Offset { get; set; } 
    }
}
