using DataManagement.Enums;
using DataManagement.Interfaces;

namespace DataManagement.DAO
{
    internal class MySqlOperation : Operation
    {
        public MySqlOperation() : base()
        {
            ConnectionType = ConnectionTypes.MySQL;
            Creator = new MySqlCreation();
        }
    }
}
