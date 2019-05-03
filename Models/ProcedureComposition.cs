using DataManagement.Attributes;

namespace DataManagement.Models
{
    public class ProcedureComposition
    {
        public string Procedure { get; set; }
        [HeaderName("sql_mode")]
        public string SqlMode { get; set; }
        [HeaderName("Create Procedure")]
        public string Definition { get; set; }
        [HeaderName("character_set_client")]
        public string CharacterSetClient { get; set; }
        [HeaderName("collation_connection")]
        public string CollationCollation { get; set; }
        [HeaderName("Database Collation")]
        public string DatabaseCollation { get; set; }
    }
}
