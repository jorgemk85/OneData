using OneData.Attributes;
using OneData.DAO;
using OneData.Interfaces;
using System;

namespace OneData.Models.Test
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
