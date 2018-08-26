using System;

namespace DataManagement.Standard.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheEnabled : Attribute
    {
        public long Expiration { get; set; }

        public CacheEnabled(int expiration)
        {
            Expiration = expiration;
        }
    }
}
