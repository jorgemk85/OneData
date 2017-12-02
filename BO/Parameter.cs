using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.BO
{
    public class Parameter
    {
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }

        public Parameter(string name, object value)
        {
            PropertyName = name;
            PropertyValue = value;
        }
    }
}
