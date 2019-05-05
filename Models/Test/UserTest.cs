using OneData.Attributes;
using OneData.Interfaces;
using System;

namespace OneData.Models.Test
{
    [DataTable("UserTests")]
    public class UserTest : Cope<UserTest>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }

        public string Nombre { get; set; }
    }
}
