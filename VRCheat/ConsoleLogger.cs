using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VRCheat
{
    public class ConsoleLogger : ILogger, ILogHandler
    {
        public ILogHandler logHandler { get; set; }
        public bool logEnabled { get; set; }
        public LogType filterLogType { get; set; }

        public ConsoleLogger() { }
        public ConsoleLogger(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
            this.logEnabled = true;
            this.filterLogType = LogType.Log;
        }

        public bool IsLogTypeAllowed(LogType logType) => true;

        public void Log(LogType logType, object message)
            => Console.WriteLine("[{0}] {1}", logType, message);

        public void Log(LogType logType, object message, UnityEngine.Object context)
            => Log(logType, message);

        public void Log(LogType logType, string tag, object message)
            => Log(logType, message);

        public void Log(LogType logType, string tag, object message, UnityEngine.Object context)
            => Log(logType, message);

        public void Log(object message)
            => Log(LogType.Log, message);

        public void Log(string tag, object message)
            => Log(message);

        public void Log(string tag, object message, UnityEngine.Object context)
            => Log(message);

        public void LogError(string tag, object message)
            => Log(message);

        public void LogError(string tag, object message, UnityEngine.Object context)
            => Log(message);

        public void LogException(Exception exception)
            => Log(LogType.Exception, exception.Message);

        public void LogException(Exception exception, UnityEngine.Object context)
            => Log(exception);

        public void LogFormat(LogType logType, string format, params object[] args)
            => Log(logType, string.Format(format, args));

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            => Log(logType, string.Format(format, args));

        public void LogWarning(string tag, object message)
            => Log(message);

        public void LogWarning(string tag, object message, UnityEngine.Object context)
            => Log(message);
    }
}
