using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for determining providers by region.
    /// </summary>
    public static class ServiceRegionUtils
    {
        internal const string DefaultRegion = "default";
        internal const string ChinaRegion = "cn";
        internal const string UKRegion = "gb";

        /// <summary>
        /// Region Providers.
        /// </summary>
        public enum Provider
        {
            /// <summary>
            /// Default provider.
            /// </summary>
            Default,

            /// <summary>
            /// GCP provider.
            /// </summary>
            GCP,

            /// <summary>
            /// Tencent provider.
            /// </summary>
            Tencent,

            /// <summary>
            /// GCPUK provider.
            /// </summary>
            GCPUK
        }

        /// <summary>
        /// Returns the Default Provider (Provider.GCP)
        /// </summary>
        public static Provider DefaultProvider => ProviderForRegion(DefaultRegion);

        /// <summary>
        /// Returns a Provider based on ISO Region Name
        /// </summary>
        public static Provider UserLocaleProvider =>
            // Force GCP until new environments are deployed
            Provider.GCP;

        static Provider ProviderForRegion(string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (string.Equals(region, ChinaRegion, StringComparison.InvariantCultureIgnoreCase))
            {
                return Provider.Tencent;
            }

            if (string.Equals(region, UKRegion, StringComparison.InvariantCultureIgnoreCase))
            {
                return Provider.GCPUK;
            }

            return Provider.GCP;
        }

        #pragma warning disable S1144 // Remove the unused private method
        // Currently unused but likely to be in the future
        static string GetLocaleSetting(string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (string.Equals(region, ChinaRegion, StringComparison.InvariantCultureIgnoreCase))
            {
                return ChinaRegion;
            }

            if (string.Equals(region, UKRegion, StringComparison.InvariantCultureIgnoreCase))
            {
                return UKRegion;
            }
            return DefaultRegion;
        }
        #pragma warning restore S1144
    }
}
