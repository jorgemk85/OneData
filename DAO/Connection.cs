using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DataManagement.DAO
{
    internal class Connection
    {
        public static string ConnectionString { get; set; }

        public static MySqlConnection OpenConnection(bool useAppConfig)
        {
            MySqlConnection connection = null;
            try
            {
                if (useAppConfig)
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectionToUse"]].ConnectionString;
                }

                connection = new MySqlConnection(ConnectionString);
                connection.Open();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            return connection;
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            connection?.Close();
        }

        public static SqlConnection OpenMSSQLConnection(bool useAppConfig)
        {
            SqlConnection connection = null;
            try
            {
                if (useAppConfig)
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectionToUse"]].ConnectionString;
                }

                connection = new SqlConnection(ConnectionString);
                connection.Open();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            return connection;
        }

        public static void CloseConnection(SqlConnection connection)
        {
            connection?.Close();
        }
    }
}
