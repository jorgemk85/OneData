using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using System;
using System.Collections.Generic;

namespace DataManagement.Models.Test
{
    [DataTable("Posts", "operaciones"), CacheEnabled(360)]
    public class Post : Cope<Post>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }

        public string Name { get; set; }

        [ForeignKey(typeof(Blog))]
        public Guid? BlogId { get; set; }
        [ForeignData(typeof(Blog))]
        public string BlogName { get; set; }

        [ForeignKey(typeof(Author))]
        public Guid? AuthorId { get; set; }
    }
}
