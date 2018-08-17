using DataManagement.Enums;
using DataManagement.Tools;
using System;
using System.Configuration;
using System.Data.Common;

namespace DataManagement.DAO
{
    internal class Connection
    {
        private static string GetConnectionString(string connectionToUse)
        {
            try
            {
                return ConsolidationTools.GetValueFromConfiguration(connectionToUse, ConfigurationTypes.ConnectionString);
            }
            catch (ConfigurationErrorsException cee)
            {
                throw cee;
            }
        }

        public static DbConnection OpenConnection(string connectionToUse, ConnectionTypes connectionType)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(CreateFactory(connectionType));
            
            DbConnection connection = null;
            try
            {
                connection = factory.CreateConnection();
                connection.ConnectionString = GetConnectionString(connectionToUse);
                connection.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return connection;
        }

        private static string CreateFactory(ConnectionTypes connectionType)
        {
            switch (connectionType)
            {
                case ConnectionTypes.MySQL:
                    return "MySql.Data.MySqlClient";
                case ConnectionTypes.MSSQL:
                    return "System.Data.SqlClient";
                default:
                    return "System.Data.SqlClient";
            }
        }

        public static void CloseConnection(DbConnection connection)
        {
            connection?.Close();
        }
    }
}
