using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An implementation of IHttpClient for .Net specific applications
    /// </summary>
    public class DotNetHttpClient : IHttpClient, IDisposable
    {
        HttpClient m_HttpClient;

        /// <summary>
        /// Initializes and returns an instance of <see cref="DotNetHttpClient"/>.
        /// </summary>
        public DotNetHttpClient()
        {
            m_HttpClient = new System.Net.Http.HttpClient();
        }

        /// <inheritdoc/>
        public TimeSpan Timeout
        {
            get => m_HttpClient.Timeout;
            set => m_HttpClient.Timeout = value;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await TrySendRequestAsync(request, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // We clone the request and re-send it to bypass the InvalidOperationException
                return await TrySendRequestAsync(await CloneRequest(request), cancellationToken);
            }
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> DownloadFileAsync(HttpRequestMessage request, string downloadFilePath,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensure internal disposal of any IDisposable references.
        /// </summary>
        /// <param name="disposing">Dispose pattern boolean value received from public Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_HttpClient?.Dispose();
                m_HttpClient = null;
            }
        }

        /// <summary>
        /// Ensure disposal of any IDisposable references.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        async Task<HttpResponseMessage> TrySendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response;

            try
            {
                response = await m_HttpClient.SendAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                if (exception is TaskCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException(exception.Message);

                    throw new TimeoutException(exception.Message);
                }

                if (exception is HttpRequestException && exception.InnerException is WebException webException)
                {
                    if (webException.Status == WebExceptionStatus.Timeout)
                        throw new TimeoutException(webException.Message);
                }

                throw;
            }

            return response;
        }

        async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            clone.Content = await CloneContent(request.Content);
            clone.Version = request.Version;

            foreach (var property in request.Properties)
                clone.Properties.Add(property);

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }

        async Task<HttpContent> CloneContent(HttpContent content)
        {
            if (content == null)
                return null;

            var memoryStream = new MemoryStream();
            await content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var clone = new StreamContent(memoryStream);
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                clone.Headers.Add(header.Key, header.Value);

            return clone;
        }
    }
}
