using DataManagement.Standard.DAO;
using log4net;
using System;
using System.Runtime.CompilerServices;

namespace DataManagement.Standard.Tools
{
    internal static class Logger
    {
        public static void Error(Exception ex, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger("DataManagement.Standard.Standard", callerName).Error(ex);
            }
        }

        public static void Warn(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger("DataManagement.Standard.Standard", callerName).Warn(message);
            }
        }

        public static void Info(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger("DataManagement.Standard.Standard", callerName).Info(message);
            }
        }

        public static void Debug(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger("DataManagement.Standard.Standard", callerName).Debug(message);
            }
        }
    }
}
