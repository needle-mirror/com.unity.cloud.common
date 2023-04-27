using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for IHttpClient
    /// </summary>
    public static class HttpClientJsonExtensions
    {
        const string ContentTypeJson = "application/json";

        static HttpContent GetJsonContent(object payload)
        {
            HttpContent content = null;
            if (payload != default)
            {
                var json = JsonSerialization.Serialize(payload);
                content = new StringContent(json, Encoding.UTF8, ContentTypeJson);
            }
            return content;
        }

        /// <summary>
        /// Asynchronously performs a Get-request using Json to deserialize response to a <typeparamref name="TModel"/>
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <typeparam name="TModel">The type the response is deserialized to</typeparam>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the <typeparamref name="TModel"/> once the request is completed</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static async Task<TModel> GetJsonAsync<TModel>(this IHttpClient httpClient, string requestUri, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);
            return await response.JsonDeserializeAsync<TModel>();
        }

        /// <summary>
        /// Asynchronously performs a Post-request, using Json to serialize payload and deserialize response to a <typeparamref name="TModel"/>
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <typeparam name="TModel">The type the response is deserialized to</typeparam>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <param name="payload">Optional <see cref="object"/> that will be used as content of the request message</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the <typeparamref name="TModel"/> once the request is completed</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static async Task<TModel> PostJsonAsync<TModel>(this IHttpClient httpClient, string requestUri, object payload = null, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync(requestUri, GetJsonContent(payload), null, cancellationToken);
            return await response.JsonDeserializeAsync<TModel>();
        }

        /// <summary>
        /// Asynchronously performs a Put-request, using Json to serialize payload and deserialize response to a <typeparamref name="TModel"/>
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <typeparam name="TModel">The type the response is deserialized to</typeparam>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <param name="payload">Optional <see cref="object"/> that will be used as content of the request message</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the <typeparamref name="TModel"/> once the request is completed</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static async Task<TModel> PutJsonAsync<TModel>(this IHttpClient httpClient, string requestUri, object payload = null, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PutAsync(requestUri, GetJsonContent(payload), cancellationToken);
            return await response.JsonDeserializeAsync<TModel>();
        }

        /// <summary>
        /// Asynchronously performs a Patch-request, using Json to serialize payload and deserialize response to a <typeparamref name="TModel"/>
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <typeparam name="TModel">The type the response is deserialized to</typeparam>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <param name="payload">Optional <see cref="object"/> that will be used as content of the request message</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the <typeparamref name="TModel"/> once the request is completed</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static async Task<TModel> PatchJsonAsync<TModel>(this IHttpClient httpClient, string requestUri, object payload = null, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PatchAsync(requestUri, GetJsonContent(payload), cancellationToken);
            return await response.JsonDeserializeAsync<TModel>();
        }

        /// <summary>
        /// Asynchronously performs a Delete-request, using Json to serialize payload and deserialize response to a <typeparamref name="TModel"/>
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <typeparam name="TModel">The type the response is deserialized to</typeparam>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <param name="payload">Optional <see cref="object"/> that will be used as content of the request message</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the <typeparamref name="TModel"/> once the request is completed</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static async Task<TModel> DeleteJsonAsync<TModel>(this IHttpClient httpClient, string requestUri, object payload = null, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.DeleteAsync(requestUri, GetJsonContent(payload), cancellationToken);
            return await response.JsonDeserializeAsync<TModel>();
        }

        /// <summary>
        /// Asynchronously performs a Delete-request, using Json to serialize payload/>
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <param name="payload">Optional <see cref="object"/> that will be used as content of the request message</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the <see cref ="HttpResponseMessage"/> once the request is completed</returns>
        /// <exception cref="ArgumentException">Thrown when the requestUri is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public static Task<HttpResponseMessage> DeleteJsonAsync(this IHttpClient httpClient, string requestUri, object payload, CancellationToken cancellationToken = default)
        {
            return httpClient.DeleteAsync(requestUri, GetJsonContent(payload), cancellationToken);
        }
    }
}
