using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for determining service domain by provider.
    /// </summary>
    static class ServiceDomainUtils
    {
        internal static readonly Dictionary<ServiceDomainProvider, string> s_ServerDomainMap = new()
        {
            { ServiceDomainProvider.Azure, "transformation.unity.com" },
            { ServiceDomainProvider.GCP, "dt.unity.com" },
        };

        /// <summary>
        /// Returns a Provider based on ISO Region Name
        /// </summary>
        internal static ServiceDomainProvider UserLocaleDomainProvider => ServiceHostResolver.DefaultDomainProvider; // Force default until new regions are deployed

        internal static ServiceDomainProvider? ParseProviderValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (Enum.TryParse<ServiceDomainProvider>(value, true, out var provider))
                return provider;

            return null;
        }
    }
}
