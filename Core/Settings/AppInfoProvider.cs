using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A class which provides access to App information as registered on the Unity Cloud Portal.
    /// </summary>
    public class AppInfoProvider : IAppInfoProvider
    {
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

        /// <summary>
        /// Returns information related to an App registered on the Unity Cloud Portal.
        /// </summary>
        /// <exception cref="System.Net.Http.HttpRequestException">This exception is thrown when the request fails to complete. See returned StatusCode for more details.</exception>
        /// <exception cref="UnauthorizedException"></exception>
        /// <exception cref="ConnectionException"></exception>
        /// <exception cref="ForbiddenException"></exception>
        public async Task<AppInfo> GetAppInfoAsync(string id)
        {
            var requestUri = m_ServiceHostResolver.GetResolvedRequestUri($"/api/applications/{id}");
            var response = await m_ServiceHttpClient.GetAsync(requestUri, ServiceHttpClientOptions.SkipDefaultAuthenticationOption());
            return await response.JsonDeserializeAsync<AppInfo>();
        }

        /// <summary>
        /// Get list of all apps inside an organization.
        /// </summary>
        /// <remarks>
        /// Only apps that user has read access to will be returned.
        /// </remarks>
        /// <exception cref="System.Net.Http.HttpRequestException">This exception is thrown when the request fails to complete. See returned StatusCode for more details.</exception>
        /// <exception cref="UnauthorizedException"></exception>
        /// <exception cref="ConnectionException"></exception>
        /// <exception cref="ForbiddenException"></exception>
        public async Task<List<AppInfo>> GetAppsInfoAsync(OrganizationId userOrgId)
        {
            var requestUri = m_ServiceHostResolver.GetResolvedRequestUri($"/api/applications?orgId={userOrgId}");
            var response = await m_ServiceHttpClient.GetAsync(requestUri);
            var appInfoList = await response.JsonDeserializeAsync<AppInfoListJson>();
            return appInfoList.Applications;
        }

    }
}
