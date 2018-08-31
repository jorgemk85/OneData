using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataManagement.Models
{
    public class ManageableCollection<TKey, TProperty> : Dictionary<TKey, TProperty>, IQueryable<TProperty>
    {
        public ManageableCollection() : base() { }
        public ManageableCollection(int capacity) : base(capacity) { }

        public Expression Expression => throw new NotImplementedException();

        public Type ElementType
        {
            get { return typeof(TProperty); }
        }

        public IQueryProvider Provider => throw new NotImplementedException();

        IEnumerator<TProperty> IEnumerable<TProperty>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
