using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// An implementation of IHttpClient for Unity specific platforms
    /// </summary>
    public class UnityHttpClient : IHttpClient
    {
        readonly LegacyRequestHandler m_RequestHandler;

        /// <summary>
        /// Initializes and returns an instance of <see cref="UnityHttpClient"/>.
        /// </summary>
        public UnityHttpClient()
        {
            m_RequestHandler = new LegacyRequestHandler();
        }

        /// <inheritdoc/>
        public TimeSpan Timeout
        {
            get => m_RequestHandler.Timeout;
            set => m_RequestHandler.Timeout = value;
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return m_RequestHandler.RequestAsync(request, null, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> DownloadFileAsync(HttpRequestMessage request, string downloadFilePath,
            CancellationToken cancellationToken = default)
        {
            return m_RequestHandler.RequestAsync(request, downloadFilePath, cancellationToken);
        }
    }
}
