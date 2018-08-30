using DataManagement.Attributes;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Posts", "operaciones")]
    public class Post : Cope<Post, Guid>
    {
        public string Name { get; set; }

        [ForeignKey(typeof(Blog))]
        public Guid? BlogId { get; set; }

        [ForeignKey(typeof(Author))]
        public Guid? AuthorId { get; set; }

        public Post() : base(Guid.NewGuid()) { }

        public Post(Guid id) : base(id) { }
    }
}
