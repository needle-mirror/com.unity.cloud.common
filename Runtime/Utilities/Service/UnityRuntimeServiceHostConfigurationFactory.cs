using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// A factory class used to create a <see cref="ServiceHostConfiguration"/>.
    /// </summary>
    public static class UnityRuntimeServiceHostConfigurationFactory
    {
#if UNITY_WEBGL && !UNITY_EDITOR
/// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with default values.
        /// Any system-level overrides set via environment variables will take priority.
        /// </summary>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration Create()
        {
            var systemEnvironmentOverrideValue = ReadLocalCacheForSystemEnvironmentOverride();
            var systemProviderOverrideValue = ReadLocalCacheForSystemProviderOverride();

            systemEnvironmentOverrideValue ??= ParseUrlForSystemEnvironmentOverrideInHost(Application.absoluteURL);
            systemProviderOverrideValue ??= ParseUrlForSystemProviderOverrideInHost(Application.absoluteURL);

            var hostOverride = new ServiceHost()
            {
                EnvironmentValue = systemEnvironmentOverrideValue ,
                ProviderValue = systemProviderOverrideValue
            };

            return new ServiceHostConfiguration(hostOverride);
        }

        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with an optional application-level override for service host options.
        /// Any system-level overrides set via environment variables will take priority.
        /// </summary>
        /// <param name="applicationOverride">An application-level override value for for service host options.</param>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration CreateWithOverride(ServiceHost applicationOverride)
        {
            var systemEnvironmentOverrideValue = ReadLocalCacheForSystemEnvironmentOverride();
            var systemProviderOverrideValue = ReadLocalCacheForSystemProviderOverride();

            systemEnvironmentOverrideValue ??= ParseUrlForSystemEnvironmentOverrideInHost(Application.absoluteURL);
            systemProviderOverrideValue ??= ParseUrlForSystemProviderOverrideInHost(Application.absoluteURL);

            var hostOverride = new ServiceHost()
            {
                EnvironmentValue = systemEnvironmentOverrideValue ?? applicationOverride.EnvironmentValue,
                ProviderValue = systemProviderOverrideValue ?? applicationOverride.ProviderValue
            };

            return new ServiceHostConfiguration(hostOverride);
        }
#else
        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with default values.
        /// Any system-level overrides set via environment variables will take priority.
        /// </summary>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration Create()
        {
            return ServiceHostConfigurationFactory.Create();
        }

        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with an optional application-level for service host options.
        /// </summary>
        /// <param name="applicationOverride">An application-level override value for for service host options.</param>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration CreateWithOverride(ServiceHost applicationOverride)
        {
            return ServiceHostConfigurationFactory.CreateWithOverride(applicationOverride);
        }
#endif

        internal static string ParseUrlForSystemEnvironmentOverrideInHost(string uriString)
        {
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                if (Regex.IsMatch(uri.Host, @"^.*stg\..*\.unity\.com$", RegexOptions.IgnoreCase))
                    return ServiceEnvironment.Staging.ToString().ToLower();
                if (Regex.IsMatch(uri.Host, @"^.*test\..*\.unity\.com$", RegexOptions.IgnoreCase))
                    return ServiceEnvironment.Test.ToString().ToLower();
                if (Regex.IsMatch(uri.Host, @"^.*\.unity\.com$", RegexOptions.IgnoreCase))
                    return ServiceEnvironment.Production.ToString().ToLower();
            }
            return null;
        }

        internal static string ParseUrlForSystemProviderOverrideInHost(string uriString)
        {
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                if (uri.Host.EndsWith("dt.unity.com"))
                    return ServiceDomainProvider.GCP.ToString().ToLower();
                if (uri.Host.EndsWith("transformation.unity.com"))
                    return ServiceDomainProvider.Azure.ToString().ToLower();
            }

            return null;
        }

        internal static string ReadLocalCacheForSystemEnvironmentOverride()
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

        internal static string ReadLocalCacheForSystemProviderOverride()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            foreach (var environmentVariableName in ProviderVariableNames())
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

        internal static IEnumerable<string> ProviderVariableNames()
        {
            yield return ServiceHostConfiguration.SystemOverrideProviderVariableName;
            yield return ServiceHostConfiguration.SystemOverrideProviderVariableName.ToLower();
        }
    }
}
