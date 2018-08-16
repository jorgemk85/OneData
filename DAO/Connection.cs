using DataManagement.Enums;
using DataManagement.Tools;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.SqlClient;

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

        public static MySqlConnection OpenMySqlConnection(string connectionToUse)
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(GetConnectionString(connectionToUse));
                connection.Open();
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            return connection;
        }

        public static SqlConnection OpenMsSqlConnection(string connectionToUse)
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(GetConnectionString(connectionToUse));
                connection.Open();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            return connection;
        }

        public static void CloseConnection(SqlConnection connection)
        {
            connection?.Close();
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            connection?.Close();
        }
    }
}
