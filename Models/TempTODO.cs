using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Models
{
    public class TempTODO
    {
        public object Name { get; set; }
        public object Value { get; set; }

        public TempTODO(object name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
