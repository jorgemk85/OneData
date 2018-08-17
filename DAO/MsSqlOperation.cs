using DataManagement.Enums;
using DataManagement.Interfaces;

namespace DataManagement.DAO
{
    internal class MsSqlOperation : Operation
    {
        public MsSqlOperation() : base()
        {
            ConnectionType = ConnectionTypes.MSSQL;
            Creator = new MsSqlCreation();
        }
    }
}
