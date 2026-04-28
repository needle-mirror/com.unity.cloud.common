#nullable enable

using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// <see cref="ICertificateValidationPolicy"/> that pins TLS servers by <b>public key only</b> (SubjectPublicKeyInfo / SPKI),
    /// using a build-generated manifest loaded from Unity <c>Resources</c>. Loads all pins once at construction; no further Resources access
    /// occurs during requests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// At runtime, loads <c>{resourcesRootPath}/BuiltPinManifest</c> (JSON: version <c>v</c> and <c>hosts</c> map of SHA-256 SPKI hex pins),
    /// produced by the Editor from PEM <c>PUBLIC KEY</c> files under <c>Assets/CertificatePins/</c>. Obtain those PEM files only from a
    /// <c>trusted source</c> (for example your PKI team or a verified fetch from the real TLS endpoint); the pins you ship are
    /// only as trustworthy as that material.
    /// </para>
    /// <para>
    /// Each pin is SHA-256(SPKI DER), hex uppercase, matching the server leaf certificate’s embedded SubjectPublicKeyInfo.
    /// Hosts with no usable pins use default platform TLS validation. Only HTTPS URIs with a DNS <see cref="Uri.Host"/> are considered.
    /// Full X.509 certificate PEM (<c>BEGIN CERTIFICATE</c>) is not used as a pinning input in the project layout; use <c>PUBLIC KEY</c> PEM only.
    /// </para>
    /// </remarks>
    public sealed class PublicKeyPinningCertificateValidationPolicy : ICertificateValidationPolicy
    {
        const string k_DefaultResourcesRootPath = "CertificatePins";

        readonly SpkiPublicKeyPinTable m_PinTable;

        /// <summary>
        /// Initializes a policy using the default Resources root <c>CertificatePins</c> and manifest <c>CertificatePins/BuiltPinManifest</c>.
        /// </summary>
        public PublicKeyPinningCertificateValidationPolicy()
            : this(k_DefaultResourcesRootPath)
        {
        }

        /// <summary>
        /// Initializes a policy with an explicit Resources root path (no file extension; Unity Resources convention).
        /// The built manifest must exist at <c>{resourcesRootPath}/BuiltPinManifest</c>.
        /// </summary>
        /// <param name="resourcesRootPath">Root folder segment under <c>Resources</c> where <c>BuiltPinManifest</c> is loaded.</param>
        public PublicKeyPinningCertificateValidationPolicy(string resourcesRootPath)
        {
            m_PinTable = SpkiPublicKeyPinTable.Load(resourcesRootPath);
        }

        /// <inheritdoc />
        public bool ShouldUseCustomValidation(Uri requestUri)
        {
            if (!SpkiPublicKeyPinTable.IsDnsHttpsHost(requestUri))
                return false;

            return m_PinTable.TryGetPinsForHost(requestUri.Host, out var pins) && !pins.IsEmpty;
        }

        /// <inheritdoc />
        public bool ValidateCertificate(Uri requestUri, byte[] certificateData)
        {
            if (requestUri == null || certificateData == null || certificateData.Length == 0)
                return false;

            if (!m_PinTable.TryGetPinsForHost(requestUri.Host, out var pins) || pins.IsEmpty)
                return false;

            return SpkiPublicKeyPinTable.MatchesServerCertificateDer(certificateData, pins);
        }

        /// <inheritdoc />
        public bool ValidateDotNetCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (request?.RequestUri == null)
                return false;

            if (!m_PinTable.TryGetPinsForHost(request.RequestUri.Host, out var pins) || pins.IsEmpty)
                return false;

            if (sslPolicyErrors != SslPolicyErrors.None || certificate == null)
                return false;

            return SpkiPublicKeyPinTable.MatchesServerCertificate(certificate, pins);
        }
    }
}
