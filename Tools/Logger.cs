using NLog;
using NLog.Config;
using OneData.DAO;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OneData.Tools
{
    internal static class Logger
    {
        private static readonly NLog.Logger log;

        static Logger()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            LogFactory logFactory = new LogFactory();
            logFactory.Configuration = new XmlLoggingConfiguration(Path.ChangeExtension(thisAssembly.Location, ".nlog"), true, logFactory);

            log = logFactory.GetLogger(thisAssembly.GetName().Name);
        }

        public static void Error(Exception ex, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Error(ex, callerName);
            }
        }

        public static void Warn(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Warn($"{callerName}: {message}");
            }
        }

        public static void Info(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Info($"{callerName}: {message}");
            }
        }

        public static void Debug(string message, [CallerMemberName] string callerName = "")
        {
            if (Manager.EnableLogInFile)
            {
                log.Debug($"{callerName}: {message}");
            }
        }
    }
}
