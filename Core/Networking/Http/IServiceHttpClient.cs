using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This interface abstracts the Task of sending <see cref="HttpRequestMessage"/>.
    /// </summary>
    public interface IServiceHttpClient : IHttpClient
    {
        /// <summary>
        /// Send an asynchronous HTTP request.
        /// </summary>
        /// <param name="request">The request to be sent.</param>
        /// <param name="options">The options for the client.</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the HttpResponseMessage once the request is completed</returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, ServiceHttpClientOptions options,
            CancellationToken cancellationToken = default);
    }
}
