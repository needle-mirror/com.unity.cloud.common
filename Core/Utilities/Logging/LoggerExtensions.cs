using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helpers function for logging
    /// </summary>
    public static class LoggerExtension
    {
        /// <summary>
        /// Log a message at the <see cref="LogLevel.Trace"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        public static void LogTrace(this UCLogger logger, string message)
            => logger.Log(LogLevel.Trace, message);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Trace"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void LogTrace(this UCLogger logger, string message, params object[] messageArgs)
            => logger.Log(LogLevel.Trace, message, messageArgs);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Debug"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        public static void LogDebug(this UCLogger logger, string message)
            => logger.Log(LogLevel.Debug, message);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Debug"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void LogDebug(this UCLogger logger, string message, params object[] messageArgs)
            => logger.Log(LogLevel.Debug, message, messageArgs);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Info"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        public static void LogInfo(this UCLogger logger, string message)
            => logger.Log(LogLevel.Info, message);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Info"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void LogInfo(this UCLogger logger, string message, params object[] messageArgs)
            => logger.Log(LogLevel.Info, message, messageArgs);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Warning"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        public static void LogWarning(this UCLogger logger, string message)
            => logger.Log(LogLevel.Warning, message);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Warning"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void LogWarning(this UCLogger logger, string message, params object[] messageArgs)
            => logger.Log(LogLevel.Warning, message, messageArgs);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Error"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        public static void LogError(this UCLogger logger, string message)
            => logger.Log(LogLevel.Error, message);

        /// <summary>
        /// Log a message at the <see cref="LogLevel.Error"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void LogError(this UCLogger logger, string message, params object[] messageArgs)
            => logger.Log(LogLevel.Error, message, messageArgs);

        /// <summary>
        /// Log an <see cref="Exception"/> at the <see cref="LogLevel.Error"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception"></param>
        public static void LogError(this UCLogger logger, Exception exception)
            => logger.Log(LogLevel.Error, exception);

        /// <summary>
        /// Log an <see cref="Exception"/> at the <see cref="LogLevel.Error"/> log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception"></param>
        /// <param name="message">The log message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void LogError(this UCLogger logger, Exception exception, string message, params object[] messageArgs)
            => logger.Log(LogLevel.Error, exception, message, messageArgs);
    }
}
