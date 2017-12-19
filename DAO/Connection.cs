using MySql.Data.MySqlClient;
using System;
using System.Configuration;

namespace DataAccess.DAO
{
    public class Connection
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
            connection.Close();
        }
    }
}
