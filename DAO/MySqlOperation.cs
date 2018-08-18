using DataManagement.Enums;

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
