using System;
using NLog;

namespace Abnormal_UI.Infra
{
    internal class Helper
    {
        private static readonly Logger Logger = LogManager.GetLogger("TestToolboxLog");
        public static string Log(string log, string logString)
        {
            Logger.Debug(log);
            logString += $"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff")} {log}\n";
            return logString;
        }
        public static string Log(Exception exception, string logString)
        {
            Logger.Error(exception);
            logString += $"{exception}\n";
            return logString;
        }
    }
}
