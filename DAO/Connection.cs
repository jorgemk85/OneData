using DataManagement.Enums;
using DataManagement.Exceptions;
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
                if (PerformMySqlConnectionStringValidation(GetConnectionString(connectionToUse)))
                {
                    connection = new MySqlConnection(GetConnectionString(connectionToUse));
                    connection.Open();
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            return connection;
        }

        private static bool PerformMySqlConnectionStringValidation(string connectionString)
        {
            string trimedConnectionString = connectionString.Replace(" ", string.Empty);

            if (!trimedConnectionString.Contains("AllowUserVariables=True"))
            {
                throw new ConnectionVariableNotEnabledException("AllowUserVariables=True");
            }

            if (!trimedConnectionString.Contains("CheckParameters=False"))
            {
                throw new ConnectionVariableNotEnabledException("CheckParameters=False");
            }

            return true;
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
