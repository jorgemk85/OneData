using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.BO
{
    public class Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
