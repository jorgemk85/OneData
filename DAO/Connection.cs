using DataManagement.Exceptions;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.SqlClient;

namespace DataManagement.DAO
{
    internal class Connection
    {
        public static string ConnectionString { get; set; }

        private static void GetConnectionString()
        {
            try
            {
                if (ConfigurationManager.AppSettings["ConnectionToUse"] == null) throw new ConfigurationNotFoundException("ConnectionToUse");
                if (ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectionToUse"]] == null) throw new ConfigurationNotFoundException(ConfigurationManager.AppSettings["ConnectionToUse"]);

                ConnectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectionToUse"]].ConnectionString;
            }
            catch (ConfigurationErrorsException cee)
            {
                throw cee;
            }
        }

        public static MySqlConnection OpenMySqlConnection(bool useAppConfig)
        {
            MySqlConnection connection = null;
            try
            {
                if (useAppConfig)
                {
                    GetConnectionString();
                }

                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                //OnConnectionOpened?.Invoke(null, new ConnectionOpenedEventArgs(connection.ConnectionString));
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            return connection;
        }

        public static SqlConnection OpenMsSqlConnection(bool useAppConfig)
        {
            SqlConnection connection = null;
            try
            {
                if (useAppConfig)
                {
                    GetConnectionString();
                }

                connection = new SqlConnection(ConnectionString);
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
