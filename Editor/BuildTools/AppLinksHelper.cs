using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// A static class that helps assigning deep linking url schema to app manifests.
    /// </summary>
    public static class AppLinksHelper
    {
        static readonly List<string> k_SupportedDomains = new List<string>
        {
            "staging.services.api.unity.com",
            "services.api.unity.com"
        };

        static readonly List<string> k_AppLinksDomains = new List<string>
        {
            "*.services.api.unity.com",
        };

        /// <summary>
        /// A static list of supported domains.
        /// </summary>
        public static ReadOnlyCollection<string> SupportedDomains => k_SupportedDomains.AsReadOnly();

        /// <summary>
        /// A static list of supported app links domains.
        /// </summary>
        public static ReadOnlyCollection<string> AppLinksDomains => k_AppLinksDomains.AsReadOnly();
    }
}
