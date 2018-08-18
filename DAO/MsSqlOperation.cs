using DataManagement.Enums;

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
