using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Object that holds the information associated to a single log
    /// </summary>
    public class LogEvent
    {
        /// <summary>
        /// Initializes and returns a <see cref="LogEvent"/>.
        /// </summary>
        public LogEvent() => Timestamp = DateTime.Now;

        /// <summary>
        /// Initializes and returns a <see cref="LogEvent"/>.
        /// </summary>
        /// <param name="loggerName">The logger's name.</param>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="properties">The properties to log.</param>
        public LogEvent(string loggerName, LogLevel level, string message, Dictionary<string, object> properties = null)
            : this()
        {
            LoggerName = loggerName;
            Level = level;
            Message = message;
            Properties = properties;
        }

        /// <summary>
        /// Initializes and returns a <see cref="LogEvent"/>.
        /// </summary>
        /// <param name="loggerName">The logger's name.</param>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The log message arguments.</param>
        /// <param name="properties">The properties to log.</param>
        public LogEvent(string loggerName, LogLevel level, string message, object[] messageArgs, Dictionary<string, object> properties = null)
            : this(loggerName, level, message, properties)
        {
            MessageArgs = messageArgs;
        }

        /// <summary>
        /// Initializes and returns a <see cref="LogEvent"/>.
        /// </summary>
        /// <param name="loggerName">The logger's name.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">The properties to log.</param>
        public LogEvent(string loggerName, LogLevel level, Exception exception, Dictionary<string, object> properties = null)
            : this()
        {
            LoggerName = loggerName;
            Level = level;
            Exception = exception;
            Properties = properties;
        }

        /// <summary>
        /// Initializes and returns a <see cref="LogEvent"/>.
        /// </summary>
        /// <param name="loggerName">The logger's name.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        /// <param name="properties">The properties to log.</param>
        public LogEvent(string loggerName, LogLevel level, Exception exception, string message, Dictionary<string, object> properties = null)
            : this(loggerName, level, exception, properties)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes and returns a <see cref="LogEvent"/>.
        /// </summary>
        /// <param name="loggerName">The logger's name.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The log message arguments.</param>
        /// <param name="properties">The properties to log.</param>
        public LogEvent(string loggerName, LogLevel level, Exception exception, string message, object[] messageArgs, Dictionary<string, object> properties = null)
            : this(loggerName, level, exception, message, properties)
        {
            MessageArgs = messageArgs;
        }

        /// <summary>
        /// The timestamp for the log.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The logger's name.
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// The <see cref="LogLevel"/>.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The log message args.
        /// </summary>
        public object[] MessageArgs { get; set; }

        /// <summary>
        /// The properties to log.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// The exception to log.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
