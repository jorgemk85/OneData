using System;

namespace OneData.Models
{
    public class BinaryObjectRepresentation
    {
        public object Name { get; set; }
        public object Value { get; set; }
        public string ParameterName { get; set; }

        public BinaryObjectRepresentation(object name, object value)
        {
            Random random = new Random();

            Name = name;
            Value = value;
            ParameterName = $"@{name.ToString().Replace("[", "").Replace("]", "").Replace(".", "")}{random.Next(1000, 9999)}";
        }
    }
}
