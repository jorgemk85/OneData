using DataManagement.Attributes;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Authors", "operaciones")]
    public class Author : Cope<Author, Guid>
    {
        public string Name { get; set; }

        public Author() : base(Guid.NewGuid()) { }

        public Author(Guid id) : base(id) { }
    }
}
