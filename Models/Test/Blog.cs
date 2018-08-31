using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Blogs", "operaciones")]
    public class Blog : Cope<Blog, Guid>
    {
        public string Name { get; set; }

        [ForeignCollection(typeof(Post))]
        public ManageableCollection<Guid, Post> Posts { get; set; }

        public Blog() : base(Guid.NewGuid()) { }

        public Blog(Guid id) : base(id) { }
    }
}
