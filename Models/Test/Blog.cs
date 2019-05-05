using OneData.Attributes;
using OneData.Interfaces;
using System;
using System.Collections.Generic;

namespace OneData.Models.Test
{
    [DataTable("Blogs", "operaciones"), CacheEnabled(360)]
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
