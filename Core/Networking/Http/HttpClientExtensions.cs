using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for <see cref="IHttpClient"/>.
    /// </summary>
    public static class HttpClientExtensions
    {
        const string k_PatchMethod = "PATCH";

        /// <summary>
        /// Sends an asynchronous GET request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> GetAsync(this IHttpClient httpClient, string requestUri, CancellationToken cancellationToken = default)
        {
            return httpClient.GetAsync(CreateUri(requestUri), cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous GET request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the requestUri is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> GetAsync(this IHttpClient httpClient, Uri requestUri, CancellationToken cancellationToken = default)
        {
            return httpClient.SendAsync(CreateHttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous POST request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="downloadFilePath">The path for the downloaded file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> PostAsync(this IHttpClient httpClient, string requestUri, HttpContent content,
            string downloadFilePath = null, CancellationToken cancellationToken = default)
        {
            return httpClient.PostAsync(CreateUri(requestUri), content, downloadFilePath, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous POST request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="downloadFilePath">The path for the downloaded file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the requestUri is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> PostAsync(this IHttpClient httpClient, Uri requestUri, HttpContent content,
            string downloadFilePath = null, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = CreateHttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = content;
            return downloadFilePath is null
                ? httpClient.SendAsync(request, cancellationToken)
                : httpClient.DownloadFileAsync(request, downloadFilePath, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous PUT request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> PutAsync(this IHttpClient httpClient, string requestUri, HttpContent content,
            CancellationToken cancellationToken = default)
        {
            return httpClient.PutAsync(CreateUri(requestUri), content, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous PUT request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the requestUri is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> PutAsync(this IHttpClient httpClient, Uri requestUri, HttpContent content,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = CreateHttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = content;
            return httpClient.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous PATCH request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> PatchAsync(this IHttpClient httpClient, string requestUri, HttpContent content,
            CancellationToken cancellationToken = default)
        {
            return httpClient.PatchAsync(CreateUri(requestUri), content, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous PATCH request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the requestUri is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> PatchAsync(this IHttpClient httpClient, Uri requestUri, HttpContent content,
            CancellationToken cancellationToken = default)
        {
            // We use the HttpMethod constructor here because HttpMethod.Patch throws PlatformNotSupportedException
            HttpRequestMessage request = CreateHttpRequestMessage(new HttpMethod(k_PatchMethod), requestUri);
            request.Content = content;
            return httpClient.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous DELETE request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> DeleteAsync(this IHttpClient httpClient, string requestUri, CancellationToken cancellationToken = default)
        {
            return httpClient.DeleteAsync(CreateUri(requestUri), cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous DELETE request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the requestUri is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> DeleteAsync(this IHttpClient httpClient, Uri requestUri, CancellationToken cancellationToken = default)
        {
            return httpClient.SendAsync(CreateHttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous DELETE request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> DeleteAsync(this IHttpClient httpClient, string requestUri, HttpContent content,
            CancellationToken cancellationToken = default)
        {
            return httpClient.DeleteAsync(CreateUri(requestUri), content, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous DELETE request to the specified Uri.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The uri for the request.</param>
        /// <param name="content">The HTTP content for the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the requestUri is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> DeleteAsync(this IHttpClient httpClient, Uri requestUri, HttpContent content,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = CreateHttpRequestMessage(HttpMethod.Delete, requestUri);
            request.Content = content;
            return httpClient.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Uri"/> from a specified <see cref="string"/>.
        /// </summary>
        /// <param name="uri">The <see cref="string"/> to convert.</param>
        /// <returns>The created <see cref="Uri"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when an unhandled exception is thrown constructing the URI.</exception>
        public static Uri CreateUri(String uri)
        {
            if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out Uri result))
                throw new ArgumentException(nameof(uri));

            return result;
        }

        /// <summary>
        /// Creates an <see cref="HttpRequestMessage"/> from an <see cref="HttpMethod"/> and a <see cref="Uri"/>.
        /// </summary>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="uri">The <see cref="Uri"/> to request.</param>
        /// <returns>The created <see cref="HttpRequestMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the URI is null.</exception>
        public static HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return new HttpRequestMessage(httpMethod, uri);
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpClientHeaderModifier"/> which adds the API source headers to each request.
        /// </summary>
        /// <param name="baseHttpClient">The client for which to modify the request headers.</param>
        /// <param name="name">The API source name.</param>
        /// <param name="version">The API source version.</param>
        /// <returns>The created <see cref="HttpClientHeaderModifier"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown is <paramref name="name"/> or <paramref name="version"/> are null or empty.</exception>
        public static IHttpClient WithApiSourceHeaders(this IHttpClient baseHttpClient, string name, string version)
        {
            var logger = LoggerProvider.GetLogger(typeof(HttpClientExtensions).FullName);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));

            // Create a header based on the API source name and version.
            var apiSourceVersion = new ApiSourceVersion(name, version);
            var sourceHeaders = new Dictionary<string, string>() {{ServiceHeaderUtils.k_ApiSourceHeader, apiSourceVersion.GetHeaderValue()}};

            logger.LogInfo($"Creating a {nameof(HttpClientHeaderModifier)} to add source headers for {apiSourceVersion.GetHeaderValue()}");

            return new HttpClientHeaderModifier(baseHttpClient, sourceHeaders);
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpClientHeaderModifier"/> which adds the API source headers to each request.
        /// The source values are retrieved from the <see cref="ApiSourceVersionAttribute"/> which must be defined in the calling <see cref="Assembly"/>.
        /// </summary>
        /// <remarks>An instance of the <see cref="ApiSourceVersionAttribute"/> must be defined at the assembly-level in the calling <see cref="Assembly"/> in order
        /// for the correct API source values to be added as a header.</remarks>
        /// <param name="baseHttpClient">The client for which to modify the request headers.</param>
        /// <param name="assembly">The target assembly.</param>
        /// <returns>The created <see cref="HttpClientHeaderModifier"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> is null or the name or version defined in the retrieved <see cref="ApiSourceVersionAttribute"/> are null or white space.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ApiSourceVersionAttribute"/> does not exist or is not initialized in the calling assembly.</exception>
        /// <exception cref="InvalidArgumentException">Thrown if <see cref="ApiSourceVersionAttribute"/> is initialized with null or empty values in the calling assembly.</exception>
        public static IHttpClient WithApiSourceHeadersFromAssembly(this IHttpClient baseHttpClient, Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var apiSourceVersion = ApiSourceVersion.GetApiSourceVersionForAssembly(assembly ?? Assembly.GetCallingAssembly());
            return baseHttpClient.WithApiSourceHeaders(apiSourceVersion.Name, apiSourceVersion.Version);
        }
    }
}
