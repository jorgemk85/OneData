namespace OneData.Models
{
    public class BinaryObjectRepresentation
    {
        public object Name { get; set; }
        public object Value { get; set; }
        public string ParameterName { get; set; }

        public BinaryObjectRepresentation(object name, object value)
        {
            Name = name;
            Value = value;
            ParameterName = $"@{name.ToString().Replace("[", "").Replace("]", "").Replace(".", "")}";
        }
    }
}
