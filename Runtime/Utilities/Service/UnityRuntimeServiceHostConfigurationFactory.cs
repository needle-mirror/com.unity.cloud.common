using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// A factory class used to create a <see cref="ServiceHostConfiguration"/>.
    /// </summary>
    public static class UnityRuntimeServiceHostConfigurationFactory
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        public static ServiceHostConfiguration Create(string applicationOverrideValue = "")
        {
            string systemOverrideValue = ReadLocalCacheForSystemOverride();

            systemOverrideValue ??= ParseUrlForSystemOverrideInHost(Application.absoluteURL);

            return systemOverrideValue == null ? new ServiceHostConfiguration(applicationOverrideValue) : new ServiceHostConfiguration(systemOverrideValue, applicationOverrideValue);
        }
#else
        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with an optional application override value for the service environment.
        /// </summary>
        /// <param name="applicationOverrideValue">An override value for the service environment.</param>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration Create(string applicationOverrideValue = "")
        {
            return ServiceHostConfigurationFactory.Create(applicationOverrideValue);
        }
#endif

        internal static string ParseUrlForSystemOverrideInHost(string uriString)
        {
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                if (uri.Host.EndsWith("stg.dt.unity.com"))
                    return ServiceEnvironment.Staging.ToString().ToLower();
                if (uri.Host.EndsWith("test.dt.unity.com"))
                    return ServiceEnvironment.Test.ToString().ToLower();
                if (uri.Host.EndsWith("dt.unity.com"))
                    return ServiceEnvironment.Production.ToString().ToLower();
            }
            return null;
        }

        internal static string ReadLocalCacheForSystemOverride()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            foreach (var environmentVariableName in EnvironmentVariableNames())
            {
                var cacheValue = CommonBrowserInterop.RetrieveCachedValue(environmentVariableName);
                if (!string.IsNullOrEmpty(cacheValue))
                    return cacheValue;
            }
#endif
            return null;
        }

        internal static IEnumerable<string> EnvironmentVariableNames()
        {
            yield return ServiceHostConfiguration.SystemOverrideEnvironmentVariableName;
            yield return ServiceHostConfiguration.SystemOverrideEnvironmentVariableName.ToLower();
        }
    }
}
