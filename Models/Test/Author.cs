using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Authors", "operaciones")]
    public class Author : Cope<Author>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }

        public string Name { get; set; }

        public Author()
        {
            Id = Guid.NewGuid();
        }

        public Author(Guid id)
        {
            Id = id;
        }
    }
}
