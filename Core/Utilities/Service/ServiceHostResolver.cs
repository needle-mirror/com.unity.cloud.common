using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    /// <inheritdoc/>
    public class ServiceHostResolver : IServiceHostResolver
    {
        const ServiceEnvironment k_DefaultEnvironment = ServiceEnvironment.Production;
        const ServiceDomainProvider k_DefaultDomainProvider = ServiceDomainProvider.Azure;

        /// <summary>
        /// Returns the Default Environment.
        /// </summary>
        internal static ServiceEnvironment DefaultEnvironment => k_DefaultEnvironment;

        /// <summary>
        /// Returns the Default Provider.
        /// </summary>
        internal static ServiceDomainProvider DefaultDomainProvider => k_DefaultDomainProvider;

        /// <summary>
        /// The environment variable key for the service environment override.
        /// </summary>
        internal static string SystemOverrideEnvironmentVariableName => "UNITY_CLOUD_SERVICES_ENV";

        /// <summary>
        /// The environment variable key for the service domain provider override.
        /// </summary>
        internal static string SystemOverrideDomainProviderVariableName => "UNITY_CLOUD_SERVICES_DOMAIN_PROVIDER";

        static readonly UCLogger s_Logger = LoggerProvider.GetLogger(typeof(ServiceHostResolver).FullName);

        readonly ServiceEnumOverride<ServiceEnvironment> m_EnvironmentOverride = new();
        readonly ServiceEnumOverride<ServiceDomainProvider> m_ProviderOverride = new();

        readonly IHttpRequestUriModifier m_HttpRequestUriModifier;

        internal ServiceHostResolver(ServiceHost? applicationOverride = null)
            : this(ReadSystemOverrides(), applicationOverride ?? new ServiceHost())
        {}

        ServiceHostResolver(ServiceHost systemOverride, ServiceHost applicationOverride)
        {
            m_EnvironmentOverride.ResolveOverride(systemOverride.EnvironmentValue, applicationOverride.EnvironmentValue, ServiceEnvironmentUtils.ParseEnvironmentValue);
            m_ProviderOverride.ResolveOverride(systemOverride.ProviderValue, applicationOverride.ProviderValue, ServiceDomainUtils.ParseProviderValue);

            m_HttpRequestUriModifier = HttpRequestUriModifierFactory.CreateFromEnvironmentVariable();
            if (m_HttpRequestUriModifier != null)
                s_Logger.LogInfo($"Installing {nameof(HttpRequestUriModifier)} on {nameof(ServiceHostResolver)}.");
        }

        static ServiceHost ReadSystemOverrides()
        {
            return new ServiceHost()
            {
                EnvironmentValue = Environment.GetEnvironmentVariable(SystemOverrideEnvironmentVariableName),
                ProviderValue = Environment.GetEnvironmentVariable(SystemOverrideDomainProviderVariableName)
            };
        }

        /// <inheritdoc/>
        public ServiceEnvironment GetResolvedEnvironment()
        {
            return m_EnvironmentOverride.Result ?? k_DefaultEnvironment;
        }

        /// <inheritdoc/>
        public ServiceDomainProvider GetResolvedDomainProvider()
        {
            return m_ProviderOverride.Result ?? k_DefaultDomainProvider;
        }

        /// <inheritdoc/>
        public string GetResolvedAddress(ServiceProtocol protocol = ServiceProtocol.Http)
        {
            var environmentOverride = GetResolvedEnvironment();

            var uriScheme = protocol switch
            {
                ServiceProtocol.Http => "https",
                ServiceProtocol.WebSocket => "ws",
                ServiceProtocol.WebSocketSecure => "wss",
                _ => "https"
            };

            var domain = GetResolvedDomain();

            var serviceAddress = environmentOverride switch
            {
                ServiceEnvironment.Production => $"{uriScheme}://{domain}",
                ServiceEnvironment.Staging => $"{uriScheme}://stg.{domain}",
                ServiceEnvironment.Test => $"{uriScheme}://test.{domain}",
                _ => throw new ArgumentOutOfRangeException(nameof(environmentOverride), environmentOverride, "Invalid environment for GetServiceAddress")
            };

            if (m_HttpRequestUriModifier != null)
                serviceAddress = m_HttpRequestUriModifier.Modify(serviceAddress);

            return serviceAddress;
        }

        /// <inheritdoc/>
        public string GetResolvedRequestUri(string path, ServiceProtocol protocol = ServiceProtocol.Http)
        {
            var requestUri = $"{GetResolvedAddress(protocol)}{path}";

            if (m_HttpRequestUriModifier != null)
                requestUri = m_HttpRequestUriModifier.Modify(requestUri);

            return requestUri;
        }

        string GetResolvedDomain()
        {
            var resolvedProvider = m_ProviderOverride.Result ?? DefaultDomainProvider;

            try
            {
                return ServiceDomainUtils.s_ServerDomainMap[resolvedProvider];
            }
            catch (KeyNotFoundException)
            {
                throw new NotSupportedException($"The service domain provider is not supported: {resolvedProvider}");
            }
        }
    }
}
