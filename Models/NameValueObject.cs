using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Models
{
    public class NameValueObject
    {
        public object Name { get; set; }
        public object Value { get; set; }

        public NameValueObject(object name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
