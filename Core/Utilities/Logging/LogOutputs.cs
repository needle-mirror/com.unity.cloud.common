using System.Collections.Generic;
using System.Linq;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Manages all log outputs
    /// </summary>
    public static class LogOutputs
    {
        static List<ILogOutput> s_Outputs = new List<ILogOutput>();

        /// <summary>
        /// The current list of <see cref="ILogOutput"/>.
        /// </summary>
        public static IReadOnlyList<ILogOutput> Outputs => s_Outputs;

        /// <summary>
        /// Adds a <see cref="ILogOutput"/> to the list of outputs.
        /// </summary>
        /// <param name="logOutput">The log output to add.</param>
        public static void Add(ILogOutput logOutput) => s_Outputs.Add(logOutput);

        /// <summary>
        /// Removes a <see cref="ILogOutput"/> from the list of outputs.
        /// </summary>
        /// <param name="logOutput">The log output to remove.</param>
        /// <returns>Whether the output was removed from the list.</returns>
        public static bool Remove(ILogOutput logOutput) => s_Outputs.Remove(logOutput);

        /// <summary>
        /// Clear all log outputs.
        /// </summary>
        public static void Clear() => s_Outputs.Clear();

        internal static void Log(LogEvent logEvent)
        {
            var logEventLevel = logEvent.Level;
            if (logEventLevel == LogLevel.None)
                return;

            foreach (var logOutput in s_Outputs.Where(o => o.Enabled))
            {
                var logOutputLevel = logOutput.CurrentLevel;
                if (logEventLevel >= logOutputLevel)
                    logOutput.Write(logEvent);
            }
        }
    }
}
