namespace Unity.Cloud.Common
{
    /// <summary>
    /// Different levels of logging. Higher level have higher numerical value and thus are more important.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Trace log level.
        /// </summary>
        Trace,

        /// <summary>
        /// Debug log level.
        /// </summary>
        Debug,

        /// <summary>
        /// Info log level.
        /// </summary>
        Info,

        /// <summary>
        /// Warning log level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error log level.
        /// </summary>
        Error
    };
}
