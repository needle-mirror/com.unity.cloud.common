using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for <see cref="HttpResponseMessage"/>.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Deserializes the content of an <see cref="HttpResponseMessage"/> to a specified type.
        /// </summary>
        /// <param name="response">The HTTP response message to deserialize.</param>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns></returns>
        public async static Task<T> JsonDeserializeAsync<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerialization.Deserialize<T>(content);
        }
    }
}
