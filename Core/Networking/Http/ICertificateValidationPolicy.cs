using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Provides certificate validation rules for <see cref="Unity.Cloud.Common.Runtime.UnityHttpClient"/> and
    /// <see cref="DotNetHttpClient"/>.
    /// </summary>
    public interface ICertificateValidationPolicy
    {
        /// <summary>
        /// Determines whether custom certificate validation should be applied for the provided request URI.
        /// </summary>
        /// <param name="requestUri">The URI that is being requested.</param>
        /// <returns>True to use custom validation, false to use the default platform validation.</returns>
        bool ShouldUseCustomValidation(Uri requestUri);

        /// <summary>
        /// Validates certificate data for Unity HTTP requests when custom validation is enabled.
        /// </summary>
        /// <param name="requestUri">The URI that is being requested.</param>
        /// <param name="certificateData">The raw server certificate bytes.</param>
        /// <returns>True when the certificate is valid for this request, otherwise false.</returns>
        bool ValidateCertificate(Uri requestUri, byte[] certificateData);

        /// <summary>
        /// Validates the server certificate for DotNet HTTP requests when custom validation is enabled.
        /// </summary>
        /// <param name="request">The HTTP request being validated.</param>
        /// <param name="certificate">The server certificate.</param>
        /// <param name="chain">The certificate chain used for validation.</param>
        /// <param name="sslPolicyErrors">TLS policy errors reported by the platform.</param>
        /// <returns>True when the certificate is valid for this request, otherwise false.</returns>
        bool ValidateDotNetCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
    }
}
