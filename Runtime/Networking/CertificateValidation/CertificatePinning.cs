#nullable enable

using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// Provides runtime information about the certificate pinning configuration built into the application.
    /// </summary>
    public static class CertificatePinning
    {
        const string k_ManifestResourcePath = "CertificatePins/BuiltPinManifest";

        static bool? s_HasBuiltInManifest;

        /// <summary>
        /// Returns <c>true</c> if a pin manifest was compiled into the build and contains at least one pinned host.
        /// Use this to decide whether to inject <see cref="PublicKeyPinningCertificateValidationPolicy"/> into your HTTP client.
        /// </summary>
        /// <remarks>
        /// The result is cached after the first access. The manifest is produced by the Editor from PEM public-key files
        /// placed under <c>Assets/CertificatePins/</c>. If no PEM files were added or certificate pinning was disabled,
        /// this property returns <c>false</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var httpClient = CertificatePinning.HasBuiltInManifest
        ///     ? new UnityHttpClient(new PublicKeyPinningCertificateValidationPolicy())
        ///     : new UnityHttpClient();
        /// </code>
        /// </example>
        public static bool HasBuiltInManifest
        {
            get
            {
                if (s_HasBuiltInManifest.HasValue)
                    return s_HasBuiltInManifest.Value;

                s_HasBuiltInManifest = CheckManifest();
                return s_HasBuiltInManifest.Value;
            }
        }

        static bool CheckManifest()
        {
            var asset = Resources.Load<TextAsset>(k_ManifestResourcePath);
            if (asset == null)
                return false;

            return CheckManifest(asset.text);
        }

        /// <summary>
        /// Core manifest check, decoupled from <see cref="Resources"/> for testability.
        /// </summary>
        /// <param name="json">Raw JSON content of the manifest, or <c>null</c> if absent.</param>
        internal static bool CheckManifest(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            var dto = JsonSerialization.Deserialize<BuiltCertificatePinManifestDto>(json);
            return dto?.hosts != null && dto.hosts.Count > 0;
        }

        /// <summary>
        /// Resets the cached value of <see cref="HasBuiltInManifest"/>.
        /// Intended for use in tests only.
        /// </summary>
        internal static void Reset() => s_HasBuiltInManifest = null;
    }
}
