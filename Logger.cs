using System;
using System.Diagnostics;
using System.Text;

namespace NDeproxy
{
    public class Logger
    {
        public static readonly StringBuilder _sb = new StringBuilder();

        public enum LogLevel { ERROR = 0, WARN = 1, INFO = 2, DEBUG = 3 };

        public LogLevel LoggingLevel;
        public readonly string Name;

        public Logger(string name)
        {
            Name = name;
        }

        public void log(LogLevel level, string message, params object[] values)
        {
            if (level >= LoggingLevel)
            {
                string line = string.Format("[{0}] {2}: {1}", level, string.Format(message, values), Name);

                Console.WriteLine(line);
                Debug.WriteLine(line);
                _sb.AppendLine(line);
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

