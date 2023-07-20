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
    class ServiceHttpClientHeaderModifier : HttpClientHeaderModifier, IServiceHttpClient
    {
        readonly IServiceHttpClient m_BaseServiceClient;

        /// <summary>
        /// Creates and instance of <see cref="ServiceHttpClientHeaderModifier"/>.
        /// </summary>
        /// <param name="serviceHttpClient">The client who's requests will have headers added.</param>
        /// <param name="headers">The headers to add to each request.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceHttpClient"/> or any key in <paramref name="headers"/> is null.</exception>
        public ServiceHttpClientHeaderModifier(IServiceHttpClient serviceHttpClient, Dictionary<string, string> headers)
            : base(serviceHttpClient, headers)
        {
            m_BaseServiceClient = serviceHttpClient ?? throw new ArgumentNullException(nameof(serviceHttpClient));
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,  ServiceHttpClientOptions options, HttpCompletionOption completionOption,
            IProgress<HttpProgress> progress = default, CancellationToken cancellationToken = default)
        {
            AddHeaders(request);
            return m_BaseServiceClient.SendAsync(request, options, completionOption, progress, cancellationToken);
        }
    }
}
