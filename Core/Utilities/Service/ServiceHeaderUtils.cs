using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Helper methods for managing HTTP headers.
    /// </summary>
    public static class ServiceHeaderUtils
    {
        public const string k_ApiSourceHeader = "X-Unity-Cloud-Api-Source";
        public const string k_UnityApiPattern = @"https.*(?:[./])unity\.com/api/.*|localhost:.*\/api/.*";

        const string k_AuthScheme = "Bearer";
        const string k_AuthHeader = "Authorization";
        const string k_AppIdHeader = "X-Digital-Twins-AppId";
        const string k_ClientTraceHeader = "X-Digital-Twins-ClientTrace";
        const string k_TraceHeader = "X-Digital-Twins-Trace";
        const string k_TraceEnvVarName = "UNITY_CLOUD_TRACE";


        static Dictionary<string, string> s_HeaderToQueryMapping = new()
        {
            {k_AuthHeader, "authorization"},
            {k_AppIdHeader, "app_id"},
            {k_ClientTraceHeader, "client_trace"},
        };

        /// <summary>
        /// Add HTTP headers to the specified Uri as query arguments.
        /// </summary>
        /// <param name="uri">The Uri to add HTTP headers as queries to.</param>
        /// <param name="headers">The HTTP headers to append as queries.</param>
        /// <returns>The modified Uri.</returns>
        public static Uri AddHeadersAsQuery(this Uri uri, HttpHeaders headers)
        {
            var uriBuilder = new UriBuilder(uri);
            if (headers != null)
            {
                var query = uriBuilder.Query;
                var queryPrefix = !string.IsNullOrWhiteSpace(query) ? "&" : "?";

                foreach (var (name, values) in headers)
                {
                    if (s_HeaderToQueryMapping.TryGetValue(name, out var queryName))
                    {
                        var escapedValue = Uri.EscapeUriString(values.Aggregate((v1, v2) => $"{v1},{v2}"));

                        if (name == k_AuthHeader)
                        {
                            escapedValue = escapedValue.Remove(0, k_AuthScheme.Length);
                        }

                        query += $"{queryPrefix}{queryName}={escapedValue}";

                        queryPrefix = "&";
                    }
                }

                uriBuilder.Query = query;
            }
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Adds the HTTP headers with specific values for app Id and client trace.
        /// </summary>
        /// <param name="headers">The HTTP headers to add to.</param>
        /// <param name="appId">The app Id.</param>
        /// <param name="clientTrace">The client trace.</param>
        public static void AddAppIdAndClientTrace(this HttpHeaders headers, string appId, string clientTrace)
        {
            if (!string.IsNullOrEmpty(appId))
            {
                headers.Add(k_AppIdHeader, appId);
            }
            if (!string.IsNullOrEmpty(clientTrace))
            {
                headers.Add(k_ClientTraceHeader, clientTrace);
            }

            // Value of user's trace environment variable
            var envTraceId = Environment.GetEnvironmentVariable(k_TraceEnvVarName);
            if (!string.IsNullOrEmpty(envTraceId))
                headers.Add(k_TraceHeader, envTraceId);
        }

        /// <summary>
        /// Add the HTTP header with a specific value for authorization.
        /// </summary>
        /// <param name="headers">The HTTP headers to add to.</param>
        /// <param name="auth">The authorization value.</param>
        public static void AddAuthorization(this HttpHeaders headers, string auth)
        {
            if (headers is HttpRequestHeaders casted)
            {
                casted.Authorization = new AuthenticationHeaderValue(k_AuthScheme, auth);
            }
            else
            {
                headers.Add(k_AuthHeader, $"{k_AuthScheme} {auth}");
            }
        }

        /// <summary>
        /// Returns the data contained in the <see cref="ApiSourceVersion"/> formatted as a string for the HTTP header value.
        /// </summary>
        /// <param name="apiSourceVersion">The version information with which to generate the header value.</param>
        /// <returns>The contents <see cref="ApiSourceVersion"/> formatted as a string for the HTTP header value.</returns>
        public static string GetHeaderValue(this ApiSourceVersion apiSourceVersion) => $"{apiSourceVersion.Name}@{apiSourceVersion.Version}";


        /// <summary>
        /// Returns whether the specified URL is a Unity API URL.
        /// </summary>
        /// <param name="url">The url to verify.</param>
        /// <returns>Whether the specified URL is a Unity API URL.</returns>
        /// <remarks>Some custom headers should only be added to requests to Unity APIs.</remarks>
        internal static bool IsUnityApi(string url)
        {
            return Regex.IsMatch(url, k_UnityApiPattern, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// Returns whether the specified URI is a Unity API URL.
        /// </summary>
        /// <param name="uri">The url to verify.</param>
        /// <returns>Whether the specified URI is a Unity API URL.</returns>
        /// <remarks>Some custom headers should only be added to requests to Unity APIs.</remarks>
        internal static bool IsUnityApi(Uri uri)
        {
            return IsUnityApi(uri.ToString());
        }
    }
}

