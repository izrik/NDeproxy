using System;
using System.Diagnostics;

namespace NDeproxy
{
    public class Logger
    {
        public LogLevel LoggingLevel;

        public enum LogLevel { ERROR = 0, WARN = 1, INFO = 2, DEBUG = 3 };

        public Logger()
        {
        }

        public void log(LogLevel level, string message, params object[] values)
        {
            if (level >= LoggingLevel)
            {
                string line = string.Format("[{0}] {1}", level, string.Format(message, values));

                Console.WriteLine(line);
                Debug.WriteLine(line);
            }
        }

        public void debug(string message, params object[] values)
        {
            log(LogLevel.DEBUG, message, values);
        }

        public void info(string message, params object[] values)
        {
            log(LogLevel.INFO, message, values);
        }

        public void warn(string message, params object[] values)
        {
            log(LogLevel.WARN, message, values);
        }

        public void error(string message, params object[] values)
        {
            log(LogLevel.ERROR, message, values);
        }
    }
}

