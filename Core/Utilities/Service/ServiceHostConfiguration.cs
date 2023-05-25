using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A class representing a configuration for the service environment for the application.
    /// </summary>
    public sealed class ServiceHostConfiguration
    {
        /// <summary>
        /// The environment variable key for the service environment override.
        /// </summary>
        public static string SystemOverrideEnvironmentVariableName => "UNITY_CLOUD_SERVICES_ENV";

        /// <summary>
        /// The environment variable key for the service domain provider override.
        /// </summary>
        public static string SystemOverrideProviderVariableName => "UNITY_CLOUD_SERVICES_DOMAIN_PROVIDER";

        readonly ServiceEnumOverride<ServiceEnvironment> m_EnvironmentOverride = new();
        readonly ServiceEnumOverride<ServiceDomainProvider> m_ProviderOverride = new();

        internal ServiceHostConfiguration(ServiceHost? applicationOverride = null)
            : this(ReadSystemOverrides(), applicationOverride ?? new ServiceHost())
        {}

        ServiceHostConfiguration(ServiceHost systemOverride, ServiceHost applicationOverride)
        {
            m_EnvironmentOverride.ResolveOverride(systemOverride.EnvironmentValue, applicationOverride.EnvironmentValue, ServiceEnvironmentUtils.ParseEnvironmentValue);
            m_ProviderOverride.ResolveOverride(systemOverride.ProviderValue, applicationOverride.ProviderValue, ServiceDomainUtils.ParseProviderValue);
        }

        static ServiceHost ReadSystemOverrides()
        {
            return new ServiceHost()
            {
                EnvironmentValue = Environment.GetEnvironmentVariable(SystemOverrideEnvironmentVariableName),
                ProviderValue = Environment.GetEnvironmentVariable(SystemOverrideProviderVariableName)
            };
        }

        /// <summary>
        /// Resolves the <see cref="ServiceEnvironment"/>, prioritizing the override set via the Environment Variable.
        /// </summary>
        /// <param name="environmentOverride">The service environment override.</param>
        /// <returns>The resolved environment and url.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public (ServiceEnvironment environment, string url) ResolveEnvironment(ServiceEnvironment? environmentOverride = null)
        {
            if (m_EnvironmentOverride.Result.HasValue)
                if (m_EnvironmentOverride.Result == ServiceEnvironment.Url)
                    return (m_EnvironmentOverride.Result.Value, m_EnvironmentOverride.OverrideValue);
                else
                    return (m_EnvironmentOverride.Result.Value, string.Empty);

            if (environmentOverride.HasValue)
            {
                if (environmentOverride.Value == ServiceEnvironment.Url)
                    throw new NotSupportedException($"Unsupported {nameof(ServiceEnvironment.Url)} per call");
                return (environmentOverride.Value, string.Empty);
            }

            return (ServiceEnvironment.Production, string.Empty);
        }

        /// <summary>
        /// Resolves the <see cref="ServiceDomainProvider"/>, prioritizing the override set via the Environment Variable.
        /// </summary>
        /// <param name="providerOverride">The service environment override.</param>
        /// <returns>The resolved environment and url.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public ServiceDomainProvider ResolveProvider(ServiceDomainProvider? providerOverride = null)
        {
            if (m_ProviderOverride.Result.HasValue)
                return m_ProviderOverride.Result.Value;

            return providerOverride ?? ServiceDomainUtils.DefaultDomainProvider;
        }

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        public string GetServiceAddress(ServiceProtocol protocol = ServiceProtocol.Http, string serviceName = "project")
            => GetServiceAddress(ServiceEnvironment.Production, ServiceDomainUtils.DefaultDomainProvider, protocol, serviceName);

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="environmentOverride">The service environment override.</param>
        /// <param name="serviceDomainProviderOverride">The service domain provider.</param>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        public string GetServiceAddress(ServiceEnvironment environmentOverride, ServiceDomainProvider serviceDomainProviderOverride, ServiceProtocol protocol = ServiceProtocol.Http, string serviceName = "project")
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

            return GetServiceAddress(environmentOverride, serviceDomainProviderOverride, subdomain, port, protocol, serviceName);
        }

        /// <summary>
        /// Returns the service address for the specified inputs.
        /// </summary>
        /// <param name="environmentOverride">The service environment override.</param>
        /// <param name="serviceDomainProviderOverride">The service domain provider.</param>
        /// <param name="subdomain">The service subdomain.</param>
        /// <param name="port">The service port.</param>
        /// <param name="protocol">The web protocol.</param>
        /// <param name="serviceName">The service's name.</param>
        /// <returns>The service address.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string GetServiceAddress(ServiceEnvironment environmentOverride, ServiceDomainProvider serviceDomainProviderOverride, string subdomain, int port, ServiceProtocol protocol, string serviceName)
        {
            string url;
            (environmentOverride, url) = ResolveEnvironment(environmentOverride);
            serviceDomainProviderOverride = ResolveProvider(serviceDomainProviderOverride);

            if (string.IsNullOrEmpty(serviceName))
                serviceName = "project";

            var uriScheme = protocol switch
            {
                ServiceProtocol.Http => environmentOverride == ServiceEnvironment.Local ? "http" : "https",
                ServiceProtocol.WebSocket => "ws",
                ServiceProtocol.WebSocketSecure => "wss",
                _ => "https"
            };

            var domain = GetServiceDomain(serviceDomainProviderOverride);

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

        /// <summary>
        /// Will return the domain for the resolved <see cref="ServiceDomainProvider"/>.
        /// </summary>
        /// <returns>The domain for the specified provider.</returns>
        public string GetServiceDomain()
        {
            return GetServiceDomain(m_ProviderOverride.Result ?? ServiceDomainUtils.DefaultDomainProvider);
        }

        /// <summary>
        /// Will return the domain for a given <see cref="ServiceDomainProvider"/>.
        /// </summary>
        /// <param name="serviceDomainProvider">The service domain provider.</param>
        /// <returns>The domain for the specified provider.</returns>
        /// <exception cref="NotSupportedException"> Thrown if the <see cref="ServiceDomainProvider"/> is not mapped to a domain.</exception>
        public string GetServiceDomain(ServiceDomainProvider serviceDomainProvider)
        {
            try
            {
                return ServiceDomainUtils.s_ServerDomainMap[serviceDomainProvider];
            }
            catch (KeyNotFoundException)
            {
                throw new NotSupportedException($"The service domain provider is not supported: {serviceDomainProvider}");
            }
        }
    }
}
