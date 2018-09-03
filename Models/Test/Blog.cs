using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;
using System.Collections.Generic;

namespace DataManagement.Models.Test
{
    [DataTable("Blogs", "operaciones"), CacheEnabled(60)]
    public class Blog : Cope<Blog>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }

        public string Name { get; set; }

        [ForeignCollection(typeof(Post))]
        public Dictionary<Guid, Post> Posts { get; set; }
    }
}
