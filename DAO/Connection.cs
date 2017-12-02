using MySql.Data.MySqlClient;
using System;
using System.Configuration;

namespace DataAccess.DAO
{
    public class Connection
    {
        public static string ConnectionString { get; set; }

        static Connection()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["main"].ConnectionString;
        }

        public static MySqlConnection OpenConnection()
        {
            MySqlConnection connection = null;
            try
            {
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
            connection.Close();
        }
    }
}
