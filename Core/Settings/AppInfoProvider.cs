using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A class which provides access to registered App information.
    /// </summary>
    public class AppInfoProvider : IAppInfoProvider
    {
        const string k_ApiVersion = "v1";
        readonly IServiceHttpClient m_ServiceHttpClient;
        readonly IServiceHostResolver m_ServiceHostResolver;

        /// <summary>
        /// Initializes and returns an instance of <see cref="AppInfoProvider"/>.
        /// </summary>
        /// <param name="serviceHttpClient">The HTTP client from which to request the app information.</param>
        /// <param name="serviceHostResolver">The service host resolver for the service Url.</param>
        public AppInfoProvider(IServiceHttpClient serviceHttpClient, IServiceHostResolver serviceHostResolver)
        {
            m_ServiceHttpClient = serviceHttpClient.WithApiSourceHeadersFromAssembly(Assembly.GetExecutingAssembly());
            m_ServiceHostResolver = serviceHostResolver;
        }

        /// <inheritdoc/>
        public async Task<AppInfo> GetAppInfoAsync(OrganizationId organizationId, AppId applicationId)
        {
            var requestUri = m_ServiceHostResolver.GetResolvedRequestUri($"/app-linking/{k_ApiVersion}/organizations/{organizationId}/applications/{applicationId}");
            var response = await m_ServiceHttpClient.GetAsync(requestUri);
            return await response.JsonDeserializeAsync<AppInfo>();
        }

        /// <inheritdoc/>
        public async Task<List<AppInfo>> GetAppsInfoAsync(OrganizationId organizationId)
        {
            var requestUri = m_ServiceHostResolver.GetResolvedRequestUri($"/app-linking/{k_ApiVersion}/organizations/{organizationId}/applications");
            var response = await m_ServiceHttpClient.GetAsync(requestUri);
            var appInfoList = await response.JsonDeserializeAsync<AppInfoListJson>();
            return appInfoList.Applications;
        }

    }
}
