using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace VOD.Lib.Libs
{
    public sealed class LogManager
    {
        public static LogManager Instance { get; } = new LogManager();
        private Logger Logger { get; }
        private LogManager()
        {
            var LogFile = Path.Combine(Environment.CurrentDirectory, "bilibili_log", "log-.txt");
            Logger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.File(LogFile, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
        public void LogError(string messageTemplate, Exception exception)
        {
            try
            {
                if (exception.IsNotEmpty())
                    Logger.Error(exception, messageTemplate);
            }
            catch { }
        }
        public void LogInfo(string messageTemplate)
        {
            try
            {
                Logger.Information(messageTemplate);
            }
            catch { }
        }
        public void LogInfo(string messageTemplate, params object[] propertyValues)
        {
            try
            {
                Logger.Information(messageTemplate, propertyValues);
            }
            catch { }
        }
    }
}