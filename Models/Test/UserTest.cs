using DataManagement.Attributes;
using DataManagement.Interfaces;
using System;

namespace DataManagement.Models.Test
{
    [DataTable("UserTests")]
    public class UserTest : Cope<UserTest>, IManageable, IIdentifiable
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
