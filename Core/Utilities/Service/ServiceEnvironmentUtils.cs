using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for service environment.
    /// </summary>
    static class ServiceEnvironmentUtils
    {
        internal static ServiceEnvironment? ParseEnvironmentValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (Uri.TryCreate(value, UriKind.Absolute, out _))
                return ServiceEnvironment.Url;

            if (Enum.TryParse<ServiceEnvironment>(value, true, out var env))
                return env;

            return null;
        }
    }
}
