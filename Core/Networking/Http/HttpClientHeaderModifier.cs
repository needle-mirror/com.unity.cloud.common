using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An implementation of an HTTP client which abstracts the Task of sending <see cref="HttpRequestMessage"/> and adds
    /// a fixed set of predefined headers to each request.
    /// </summary>
    class HttpClientHeaderModifier : IHttpClient
    {
        readonly IHttpClient m_BaseClient;
        readonly Dictionary<string, string> m_Headers;

        /// <summary>
        /// Creates and instance of <see cref="HttpClientHeaderModifier"/>.
        /// </summary>
        /// <param name="httpClient">The client who's requests will have headers added.</param>
        /// <param name="headers">The headers to add to each request.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or any key in <paramref name="headers"/> is null.</exception>
        public HttpClientHeaderModifier(IHttpClient httpClient, Dictionary<string, string> headers)
        {
            m_BaseClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            m_Headers = headers != null && !headers.Keys.Any(string.IsNullOrWhiteSpace) ? headers : throw new ArgumentNullException(nameof(headers), $"A key in {nameof(headers)} is null or white space.");
        }

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => m_BaseClient.Timeout;
            set => m_BaseClient.Timeout = value;
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            AddHeaders(request);
            return m_BaseClient.SendAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> DownloadFileAsync(HttpRequestMessage request, string downloadFilePath, CancellationToken cancellationToken = default)
        {
            AddHeaders(request);
            return m_BaseClient.DownloadFileAsync(request, downloadFilePath, cancellationToken);
        }

        protected void AddHeaders(HttpRequestMessage request)
        {
            foreach (var header in m_Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
    }
}
