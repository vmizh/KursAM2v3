using System;
using System.Text;
using System.Windows;
using JetBrains.Annotations;
using NLog;

namespace Core.Logger
{
    public static class LoggerHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly NLog.Logger kursLogger;

        static LoggerHelper()
        {
           
            kursLogger = LogManager.GetCurrentClassLogger(); 
            LogManager.Configuration.Variables["programName"] = Application.Current?.ToString();
        }

        [NotNull]
        // ReSharper disable once ConvertToAutoProperty
        public static NLog.Logger Logger => kursLogger;

        public static void WriteError(Exception ex)
        {
            var errText = new StringBuilder(ex.Message);
            var ex1 = ex;
            while (ex1.InnerException != null)
            {
                ex1 = ex1.InnerException;
                errText.Append(ex1.Message + "\n");
                if (ex1.InnerException != null)
                    errText.Append(ex1.InnerException.Message);
            }
            LogManager.Configuration.Variables["userName"] = "sysadm";
            LogManager.Configuration.Variables["programName"] = "KursRepozit";
            kursLogger.Error(errText);
        }

        public static void WriteMessage(LogLevelEnum level,string message)
        {
            switch (level)
            {
                case LogLevelEnum.Error:
                    kursLogger.Error(message);
                    break;
                case LogLevelEnum.Debug:
                    kursLogger.Debug(message);
                    break;
                case LogLevelEnum.Fatal:
                    kursLogger.Fatal(message);
                    break;
                case LogLevelEnum.Info:
                    kursLogger.Info(message);
                    break;
                case LogLevelEnum.None:
                    return;
                case LogLevelEnum.Trace:
                    kursLogger.Trace(message);
                    break;
                case LogLevelEnum.Warning:
                    kursLogger.Warn(message);
                    break;
            }
        }
    }
}