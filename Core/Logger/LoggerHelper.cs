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

        static LoggerHelper()
        {
            Logger = LogManager.GetCurrentClassLogger();
            LogManager.Configuration.Variables["programName"] = Application.Current?.ToString();
        }

        [NotNull]
        // ReSharper disable once ConvertToAutoProperty
        public static NLog.Logger Logger { get; }

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
            Logger.Error(errText);
        }

        public static void WriteMessage(LogLevelEnum level, string message)
        {
            switch (level)
            {
                case LogLevelEnum.Error:
                    Logger.Error(message);
                    break;
                case LogLevelEnum.Debug:
                    Logger.Debug(message);
                    break;
                case LogLevelEnum.Fatal:
                    Logger.Fatal(message);
                    break;
                case LogLevelEnum.Info:
                    Logger.Info(message);
                    break;
                case LogLevelEnum.None:
                    return;
                case LogLevelEnum.Trace:
                    Logger.Trace(message);
                    break;
                case LogLevelEnum.Warning:
                    Logger.Warn(message);
                    break;
            }
        }
    }
}