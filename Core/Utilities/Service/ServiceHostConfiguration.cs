using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A factory class used to create a <see cref="ServiceHostConfiguration"/>.
    /// </summary>
    public static class ServiceHostConfigurationFactory
    {
        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with an optional application override value for the service environment.
        /// </summary>
        /// <param name="applicationOverrideValue">An override value for the service environment.</param>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration Create(string applicationOverrideValue = null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            return new ServiceHostConfiguration(applicationOverrideValue);
#else
            throw new InvalidOperationException("Consider using UnityRuntimeServiceHostConfigurationFactory from the Unity.Cloud.Common.Runtime assembly");
#endif
        }
    }

    /// <summary>
    /// A class representing a configuration for the service environment for the application.
    /// </summary>
    public sealed class ServiceHostConfiguration
    {
        static readonly UCLogger s_Logger = LoggerProvider.GetLogger<ServiceHostConfiguration>();

        /// <summary>
        /// The environment variable key for the service environment override.
        /// </summary>
        public static string SystemOverrideEnvironmentVariableName => "UNITY_CLOUD_SERVICES_ENV";

        static readonly Dictionary<ServiceRegionUtils.Provider, string> s_ServerDomainMap = new()
        {
            { ServiceRegionUtils.Provider.GCP, "dt.unity.com" },
            { ServiceRegionUtils.Provider.Tencent, "dt.unity.cn" },
            { ServiceRegionUtils.Provider.GCPUK, "uk.dt.unity.com" }
        };

        readonly string m_OverrideValue;
        readonly ServiceEnvironment? m_OverrideEnvironment;

        internal ServiceHostConfiguration(string applicationOverrideValue = null)
            : this(ReadEnvironmentVariable(), applicationOverrideValue)
        {}

        internal ServiceHostConfiguration(string systemOverrideValue, string applicationOverrideValue)
        {
            m_OverrideValue = string.Empty;
            m_OverrideEnvironment = null;

            var systemOverrideEnvironment = ResolveDefault(ParseEnvironmentValue(systemOverrideValue));
            if (systemOverrideEnvironment.HasValue)
            {
                m_OverrideValue = systemOverrideValue;
                m_OverrideEnvironment = systemOverrideEnvironment;

                s_Logger.LogInfo($"{nameof(ServiceHostConfiguration)} created with system override value: {m_OverrideValue} and environment {m_OverrideEnvironment}");
            }
            else
            {
                var applicationEnvironment = ResolveDefault(ParseEnvironmentValue(applicationOverrideValue));
                if (applicationEnvironment.HasValue)
                {
                    m_OverrideValue = applicationOverrideValue;
                    m_OverrideEnvironment = applicationEnvironment;

                    s_Logger.LogInfo($"{nameof(ServiceHostConfiguration)} created with application override value: {m_OverrideValue} and environment {m_OverrideEnvironment}");
                }
                else
                    s_Logger.LogInfo($"{nameof(ServiceHostConfiguration)} created without override value");
            }

        }

        static ServiceEnvironment? ResolveDefault(ServiceEnvironment? environment)
        {
            if (environment.HasValue)
                return environment.Value == ServiceEnvironment.Default ? ServiceEnvironment.Production : environment;

            return null;
        }

        static ServiceEnvironment? ParseEnvironmentValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (Uri.TryCreate(value, UriKind.Absolute, out _))
                return ServiceEnvironment.Url;

            if (Enum.TryParse<ServiceEnvironment>(value, true, out var env))
                return env;

            return null;
        }

        static string ReadEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable(SystemOverrideEnvironmentVariableName);
        }

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="environmentOverride">The service environment override.</param>
        /// <returns>The resolved environment and url.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public (ServiceEnvironment environment, string url) ResolveEnvironment(ServiceEnvironment? environmentOverride = null)
        {
            if (m_OverrideEnvironment.HasValue)
                if (m_OverrideEnvironment == ServiceEnvironment.Url)
                    return (m_OverrideEnvironment.Value, m_OverrideValue);
                else
                    return (m_OverrideEnvironment.Value, string.Empty);

            environmentOverride = ResolveDefault(environmentOverride);
            if (environmentOverride.HasValue)
            {
                if (environmentOverride.Value == ServiceEnvironment.Url)
                    throw new NotSupportedException($"Unsupported {nameof(ServiceEnvironment.Url)} per call");
                return (environmentOverride.Value, string.Empty);
            }

            return (ServiceEnvironment.Production, string.Empty);
        }

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        public string GetServiceAddress(ServiceProtocol protocol = ServiceProtocol.Http, string serviceName = "project")
            => GetServiceAddress(ServiceRegionUtils.UserLocaleProvider, protocol, serviceName);

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="regionProvider">The region provider.</param>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        public string GetServiceAddress(ServiceRegionUtils.Provider regionProvider, ServiceProtocol protocol = ServiceProtocol.Http, string serviceName = "project")
            => GetServiceAddress(ServiceEnvironment.Default, regionProvider, protocol, serviceName);

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="environmentOverride">The service environment override.</param>
        /// <param name="regionProvider">The region provider.</param>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        public string GetServiceAddress(ServiceEnvironment environmentOverride, ServiceRegionUtils.Provider regionProvider, ServiceProtocol protocol = ServiceProtocol.Http, string serviceName = "project")
        {
            var port = 10010;
            var subdomain = "";

            if (protocol == ServiceProtocol.Http)
            {
                port = 5555;
            }
            else if (protocol is ServiceProtocol.WebSocket or ServiceProtocol.WebSocketSecure)
            {
                port = 5000;
            }

            return GetServiceAddress(environmentOverride, regionProvider, subdomain, port, protocol, serviceName);
        }

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="environmentOverride">The service environment override.</param>
        /// <param name="regionProvider">The region provider.</param>
        /// <param name="subdomain">The service subdomain.</param>
        /// <param name="port">The service port.</param>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string GetServiceAddress(ServiceEnvironment environmentOverride, ServiceRegionUtils.Provider regionProvider, string subdomain, int port, ServiceProtocol protocol, string serviceName)
        {
            string url;
            (environmentOverride, url) = ResolveEnvironment(environmentOverride);

            if (string.IsNullOrEmpty(serviceName))
                serviceName = "project";

            if (environmentOverride == ServiceEnvironment.Default)
                environmentOverride = ServiceEnvironment.Production;
            if (regionProvider == ServiceRegionUtils.Provider.Default)
                regionProvider = ServiceRegionUtils.Provider.GCP;

            if (!regionProvider.Equals(ServiceRegionUtils.Provider.GCP))
            {
                throw new NotSupportedException($"Unsupported Region '{regionProvider}' Please use ServiceRegionUtils.Provider.GCP.");
            }

            var uriScheme = protocol switch
            {
                ServiceProtocol.Http => environmentOverride == ServiceEnvironment.Local ? "http" : "https",
                ServiceProtocol.WebSocket => "ws",
                ServiceProtocol.WebSocketSecure => "wss",
                _ => "https"
            };

            var domain = s_ServerDomainMap[regionProvider];
            return environmentOverride switch
            {
                ServiceEnvironment.Production => $"{uriScheme}://{subdomain}{domain}",
                ServiceEnvironment.Staging => $"{uriScheme}://{subdomain}stg.{domain}",
                ServiceEnvironment.Test => $"{uriScheme}://{subdomain}test.{domain}",
                ServiceEnvironment.Local => $"{uriScheme}://uc-{serviceName}:{port}",
                ServiceEnvironment.Url => url,
                _ => throw new ArgumentOutOfRangeException(nameof(environmentOverride), environmentOverride, "Invalid environment for GetServiceAddress")
            };
        }

    }
}
