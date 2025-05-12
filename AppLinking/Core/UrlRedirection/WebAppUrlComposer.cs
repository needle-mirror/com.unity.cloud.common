using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Cloud.Common;

namespace Unity.Cloud.AppLinking
{
    /// <summary>
    /// A class that provides utilities for web app resources.
    /// </summary>
    public class WebAppUrlComposer : IWebAppUrlComposer
    {
        public static readonly string Documentation = "docs";
        public static readonly string AssetManager = "asset-manager";

        readonly string m_WebAppUrlEndpoint = "/app-linking/v1/web-app-urls";
        readonly IServiceHostResolver m_ServiceHostResolver;
        readonly IHttpClient m_HttpClient;

        Dictionary<string, string> m_WebAppBaseUrls = new ();

        public WebAppUrlComposer(IServiceHostResolver serviceHostResolver, IHttpClient httpClient)
        {
            m_ServiceHostResolver = serviceHostResolver;
            m_HttpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<string> ComposeUrlAsync(string webAppName, string pathAndQuery = null)
        {
            await FetchWebAppUrlsAsync();
            if (!await IsWebAppSupportedAsync(webAppName))
            {
                throw new InvalidArgumentException($"The web app name '{webAppName}' does not exist.");
            }
            return $"{m_WebAppBaseUrls[webAppName]}{pathAndQuery}";;
        }

        /// <inheritdoc/>
        public async Task<bool> IsWebAppSupportedAsync(string webAppName)
        {
            await FetchWebAppUrlsAsync();
            if (m_WebAppBaseUrls.TryGetValue(webAppName, out var url))
            {
                return !string.IsNullOrEmpty(url);
            }
            return false;
        }

        async Task FetchWebAppUrlsAsync()
        {
            // Fetch web app URLs once per session
            if (m_WebAppBaseUrls.Count == 0)
            {
                var webAppUrlEndpoint = m_ServiceHostResolver.GetResolvedRequestUri(m_WebAppUrlEndpoint);
                var response = await m_HttpClient.GetAsync(webAppUrlEndpoint);
                m_WebAppBaseUrls = await response.JsonDeserializeAsync<Dictionary<string, string>>();
            }
        }

    }
}
