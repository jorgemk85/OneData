using log4net;
using System;
using System.Reflection;

namespace DataManagement.Tools
{
    internal static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Error(Exception ex)
        {
            log.Error(ex);
        }

        public static void Warn(string message)
        {
            log.Warn(message);
        }

        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void Debug(string message)
        {
            log.Debug(message);
        }
    }
}
