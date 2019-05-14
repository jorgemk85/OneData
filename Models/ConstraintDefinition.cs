using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneData.Models
{
    internal sealed class ConstraintDefinition
    {
        public string Constraint_Catalog { get; set; }
        public string Constraint_Schema { get; set; }
        public string Constraint_Name { get; set; }
        public string Constraint_Type { get; set; }
        public string Table_Name { get; set; }
        public string Column_Name { get; set; }
    }
}
