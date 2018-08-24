using DataManagement.Standard.Enums;
using DataManagement.Standard.Exceptions;
using DataManagement.Standard.Tools;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.SqlClient;

namespace DataManagement.Standard.DAO
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
                Logger.Error(cee);
                throw cee;
            }
        }

        public static MySqlConnection OpenMySqlConnection(string connectionToUse)
        {
            MySqlConnection connection = null;
            try
            {
                Logger.Info("Attempting to connect to MySql Server.");
                if (PerformMySqlConnectionStringValidation(GetConnectionString(connectionToUse)))
                {
                    connection = new MySqlConnection(GetConnectionString(connectionToUse));
                    connection.Open();
                    Logger.Info("Connected.");
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error(ex);
                throw ex;
            }
            return connection;
        }

        private static bool PerformMySqlConnectionStringValidation(string connectionString)
        {
            string trimedConnectionString = connectionString.Replace(" ", string.Empty);

            if (!trimedConnectionString.Contains("AllowUserVariables=True"))
            {
                ConnectionVariableNotEnabledException cvnee = new ConnectionVariableNotEnabledException("AllowUserVariables=True");
                Logger.Error(cvnee);
                throw cvnee;
            }

            if (!trimedConnectionString.Contains("CheckParameters=False"))
            {
                ConnectionVariableNotEnabledException cvnee = new ConnectionVariableNotEnabledException("CheckParameters=False");
                Logger.Error(cvnee);
                throw cvnee;
            }

            return true;
        }

        public static SqlConnection OpenMsSqlConnection(string connectionToUse)
        {
            SqlConnection connection = null;
            try
            {
                Logger.Info("Attempting to connect to Sql Server.");
                connection = new SqlConnection(GetConnectionString(connectionToUse));
                connection.Open();
                Logger.Info("Connected.");
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
                throw ex;
            }
            return connection;
        }

        public static void CloseConnection(SqlConnection connection)
        {
            connection?.Close();
            Logger.Info("Connection closed.");
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            connection?.Close();
            Logger.Info("Connection closed.");
        }
    }
}
