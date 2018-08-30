using DataManagement.Attributes;
using System;
using System.Collections.Generic;

namespace DataManagement.Models.Test
{
    [DataTable("Blogs", "operaciones")]
    public class Blog : Cope<Blog, Guid>
    {
        public string Name { get; set; }

        [ForeignCollection(typeof(Post))]
        public ICollection<Post> Posts { get; set; }

        public Blog() : base(Guid.NewGuid()) { }

        public Blog(Guid id) : base(id) { }
    }
}
