using DataManagement.DAO;
using log4net;
using System;
using System.Runtime.CompilerServices;

namespace DataManagement.Tools
{
    internal static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Error(Exception ex, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Error(ex);
            }
        }

        public static void Warn(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Warn(message);
            }
        }

        public static void Info(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Info(message);
            }
        }

        public static void Debug(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Debug(message);
            }
        }
    }
}
