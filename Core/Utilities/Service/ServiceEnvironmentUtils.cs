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

#pragma warning disable CS0618 // Type or member is obsolete
            if (Uri.TryCreate(value, UriKind.Absolute, out _))
                return ServiceEnvironment.Url;
#pragma warning restore CS0618

            if (Enum.TryParse<ServiceEnvironment>(value, true, out var env))
                return env;

            return null;
        }
    }
}
