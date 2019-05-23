using System;

namespace OneData.Attributes
{
    /// <summary>
    /// Use this attribute to set a default value in a column inside your table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Default : Attribute
    {
        public object Value { get; set; }

        /// <summary>
        /// Use this attribute to set a default value in a column inside your table.
        /// </summary>
        /// <param name="value">The value to be set as default when you perform an 'Insert' and send null in the property that this attribute is currently set.</param>
        public Default(object value)
        {
            Value = value;
        }
    }
}
