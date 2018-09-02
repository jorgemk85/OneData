using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Authors", "operaciones")]
    public class Author : Cope<Author>, IManageable
    {
        [PrimaryProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }

        public string Name { get; set; }

    }
}
