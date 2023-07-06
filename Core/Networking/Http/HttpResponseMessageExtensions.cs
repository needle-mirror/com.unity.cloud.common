using System;
using System.IO;
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
            string content;

#if UNITY_WEBGL && !UNITY_EDITOR
            var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            content = reader.ReadToEnd();
#else
            content = await response.Content.ReadAsStringAsync();
#endif

            return JsonSerialization.Deserialize<T>(content);
        }
    }
}
