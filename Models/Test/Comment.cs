using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("Comments", "operaciones")]
    public class Comment : Cope<Comment>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }
        public string Name { get; set; }

        [ForeignKey(typeof(Post))]
        public Guid? PostId { get; set; }

    }
}
