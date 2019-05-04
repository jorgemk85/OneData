using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneData.Models
{
    internal sealed class KeyDefinition
    {
        public string Constraint_Catalog { get; set; }
        public string Constraint_Schema { get; set; }
        public string Constraint_Name { get; set; }
        public string Table_Catalog { get; set; }
        public string Table_Schema { get; set; }
        public string Table_Name { get; set; }
        public string Column_Name { get; set; }
        public int Ordinal_Position { get; set; }
        public int? Position_In_Unique_Constraint { get; set; }
        public string Referenced_Table_Schema { get; set; }
        public string Referenced_Table_Name { get; set; }
        public string Referenced_Column_Name { get; set; }
    }
}
