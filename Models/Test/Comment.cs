using DataManagement.Attributes;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Comments", "operaciones")]
    public class Comment : Cope<Comment, Guid>
    {
        public string Name { get; set; }

        [ForeignKey(typeof(Post))]
        public Guid? PostId { get; set; }

        public Comment() : base(Guid.NewGuid()) { }

        public Comment(Guid id) : base(id) { }

    }
}
