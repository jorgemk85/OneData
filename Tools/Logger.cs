using DataManagement.DAO;
using log4net;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DataManagement.Tools
{
    internal static class Logger
    {
        public static void Error(Exception ex, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger(callerName).Error(ex);
            }
        }

        public static void Warn(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger(callerName).Warn(message);
            }
        }

        public static void Info(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger(callerName).Info(message);
            }
        }

        public static void Debug(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                LogManager.GetLogger(callerName).Debug(message);
            }
        }
    }
}
