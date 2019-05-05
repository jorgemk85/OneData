using System;

namespace OneData.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataLength : Attribute
    {
        public long Length { get; set; }

        public DataLength(long length)
        {
            Length = length;
        }
    }
}
