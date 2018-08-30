using DataManagement.Attributes;

namespace DataManagement.Models.Test
{
    [DataTable("UserTests")]
    public class UserTest : Cope<UserTest, int>
    {
        public string Nombre { get; set; }

        public UserTest() : base(0) { }

        public UserTest(int id) : base(id) { }
    }
}
