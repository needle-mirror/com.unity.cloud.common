using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A class to configure options for a <see cref="IServiceHttpClient"/>
    /// </summary>
    public struct ServiceHttpClientOptions
    {
        /// <summary>
        /// Whether to skip the default authentication flow.
        /// </summary>
        public bool SkipDefaultAuthentication { get; }

        /// <summary>
        /// Whether to fill the default headers.
        /// </summary>
        public bool SkipDefaultHeaders { get; }

        /// <summary>
        /// Whether to skip error processing.
        /// </summary>
        public bool SkipErrorProcessing { get; }

        /// <summary>
        /// Whether to use the base <see cref="IHttpClient"/> to send requests.
        /// </summary>
        public bool UseBaseHttpClientOnly { get; }

        /// <summary>
        /// The path for file downloads.
        /// </summary>
        public string DownloadFilePath { get; }

        IRetryPolicy m_RetryPolicy;

        /// <summary>
        /// The <see cref="IRetryPolicy"/> to be used by the client.
        /// </summary>
        public IRetryPolicy RetryPolicy => m_RetryPolicy ??= new ExponentialBackoffRetryPolicy();

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/>.
        /// </summary>
        /// <param name="skipDefaultAuthentication">Whether to skip the default authentication flow.</param>
        /// <param name="skipDefaultHeaders">Whether to fill the default headers.</param>
        /// <param name="skipErrorProcessing">Whether to skip error processing.</param>
        /// <param name="useBaseHttpClientOnly">Whether to use the base HTTP client to send requests.</param>
        /// <param name="downloadFilePath">The path for file downloads.</param>
        /// <param name="retryPolicy">The retry policy to use for the client.</param>
        public ServiceHttpClientOptions(bool skipDefaultAuthentication, bool skipDefaultHeaders, bool skipErrorProcessing,
            bool useBaseHttpClientOnly, string downloadFilePath = null, IRetryPolicy retryPolicy = null)
        {
            SkipDefaultAuthentication = skipDefaultAuthentication;
            SkipDefaultHeaders = skipDefaultHeaders;
            SkipErrorProcessing = skipErrorProcessing;
            UseBaseHttpClientOnly = useBaseHttpClientOnly;
            DownloadFilePath = downloadFilePath;
            m_RetryPolicy = retryPolicy ?? new ExponentialBackoffRetryPolicy();
        }

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> with default settings.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions Default() => new ServiceHttpClientOptions(false, false, false, false);

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> which skips default authentication.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions SkipDefaultAuthenticationOption() => new ServiceHttpClientOptions(true, false, false, false);

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> which skips adding default headers.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions SkipDefaultHeadersOption() => new ServiceHttpClientOptions(false, true, false, false);

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> which skips error processing.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions SkipErrorProcessingOption() => new ServiceHttpClientOptions(false, false, true, false);

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> which uses the base <see cref="IHttpClient"/>.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions UseBaseHttpClientOnlyOption() => new ServiceHttpClientOptions(false, false, false, true);

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> with no retry policy.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions NoRetryOption() => new ServiceHttpClientOptions(false, false, false, false, null, new NoRetryPolicy());

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> with file download options.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions DownloadFilePathOption(string path) => new ServiceHttpClientOptions(false, false, false, false, path, null);

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClientOptions"/> with file download options and no retry policy.
        /// </summary>
        /// <returns>The specific client options.</returns>
        public static ServiceHttpClientOptions DownloadFilePathNoRetryOption(string path) => new ServiceHttpClientOptions(false, false, false, false, path, new NoRetryPolicy());
    }

    /// <summary>
    /// An implementation of an HTTP client which abstracts the Task of sending <see cref="HttpRequestMessage"/>.
    /// </summary>
    public class ServiceHttpClient : IServiceHttpClient
    {
        readonly IHttpClient m_BaseHttpClient;
        readonly IAccessTokenProvider m_AccessTokenProvider;
        readonly IAppIdProvider m_AppIdProvider;

        internal static readonly string k_ClientTrace = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Initializes and returns an instance of <see cref="ServiceHttpClient"/>
        /// </summary>
        /// <param name="baseHttpClient">The base HTTP client.</param>
        /// <param name="accessTokenProvider">The access token provider to authenticate requests.</param>
        /// <param name="appIdProvider">The App ID provider.</param>
        public ServiceHttpClient(IHttpClient baseHttpClient,
            IAccessTokenProvider accessTokenProvider,
            IAppIdProvider appIdProvider)
        {
#if UC_DEV_TOOLS
            m_BaseHttpClient = baseHttpClient.WithModifierMiddleware();
#else
            m_BaseHttpClient = baseHttpClient;
#endif
            m_AccessTokenProvider = accessTokenProvider;
            m_AppIdProvider = appIdProvider;
        }

        /// <summary>
        /// The timespan to wait before the request times out.
        /// </summary>
        public TimeSpan Timeout
        {
            get => m_BaseHttpClient.Timeout;
            set => m_BaseHttpClient.Timeout = value;
        }

        /// <summary>
        /// Send an asynchronous HTTP request.
        /// </summary>
        /// <param name="request">The request to be sent.</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the HttpResponseMessage once the request is completed</returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return SendAsync(request, ServiceHttpClientOptions.NoRetryOption(), cancellationToken);
        }

        /// <summary>
        /// Send an asynchronous HTTP request.
        /// </summary>
        /// <param name="request">The request to be sent.</param>
        /// <param name="options">The options for the client.</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the HttpResponseMessage once the request is completed</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, ServiceHttpClientOptions options,
            CancellationToken cancellationToken = default)
        {
            if (options.UseBaseHttpClientOnly)
            {
                return await SendBaseHttpClientAsync(request, options, ResponseValidatorNoErrorProcessing, cancellationToken);
            }

            if (!options.SkipDefaultHeaders)
            {
                request.Headers.AddAppIdAndClientTrace(m_AppIdProvider?.GetAppId(), k_ClientTrace);
            }

            if (!options.SkipDefaultAuthentication)
            {
                if (m_AccessTokenProvider != null)
                {
                    var accessToken = await m_AccessTokenProvider.GetAccessTokenAsync();
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        request.Headers.AddAuthorization(accessToken);
                    }
                }
            }

            if (options.SkipErrorProcessing)
            {
                return await SendBaseHttpClientAsync(request, options, ResponseValidatorNoErrorProcessing, cancellationToken);
            }

            return await SendWithErrorProcessingAsync(request, options, cancellationToken);
        }

        /// <summary>
        /// Send an asynchronous a file download request.
        /// </summary>
        /// <param name="request">The request to be sent.</param>
        /// <param name="downloadFilePath">Optional path to save downloaded files.</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the HttpResponseMessage once the request is completed</returns>
        public Task<HttpResponseMessage> DownloadFileAsync(HttpRequestMessage request, string downloadFilePath,
            CancellationToken cancellationToken = default)
        {
            return SendAsync(request, ServiceHttpClientOptions.DownloadFilePathOption(downloadFilePath), cancellationToken);
        }

        async Task<HttpResponseMessage> SendBaseHttpClientAsync(HttpRequestMessage request, ServiceHttpClientOptions options, IRetryPolicy.ShouldRetryResultChecker<HttpResponseMessage> responseValidator, CancellationToken cancellationToken = default)
        {
            return await (options.DownloadFilePath is null
                ? options.RetryPolicy.ExecuteAsyncWithResultValidation<HttpResponseMessage>(ct => m_BaseHttpClient.SendAsync(request, ct), responseValidator, cancellationToken)
                : options.RetryPolicy.ExecuteAsyncWithResultValidation<HttpResponseMessage>(ct => m_BaseHttpClient.DownloadFileAsync(request, options.DownloadFilePath, ct), responseValidator, cancellationToken));
        }

        Task<bool> ResponseValidatorNoErrorProcessing(HttpResponseMessage response)
        {
            return Task.FromResult(response.StatusCode == HttpStatusCode.RequestTimeout || (int)response.StatusCode >= 500);
        }

        async Task<HttpResponseMessage> SendWithErrorProcessingAsync(HttpRequestMessage request, ServiceHttpClientOptions options, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendBaseHttpClientAsync(request, options, ResponseValidatorNoErrorProcessing, cancellationToken);
                return await ProcessResponseErrorAsync(response);
            }
            catch (HttpRequestException e)
            {
                throw new ConnectionException(ServiceErrorMessage.ConnectionFailed, e);
            }
        }

        async Task<HttpResponseMessage> ProcessResponseErrorAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            ServiceError serviceError;
            try
            {
                var content = await response.Content.ReadAsStringAsync();

                serviceError = JsonSerialization.Deserialize<ServiceError>(content);
                serviceError.HttpStatusCode = response.StatusCode;
            }
            catch (Exception)
            {
                serviceError = new ServiceError { HttpStatusCode = response.StatusCode };

                throw ServiceExceptionFactory.Build(serviceError);
            }

            throw ServiceExceptionFactory.Build(serviceError);
        }
    }
}
