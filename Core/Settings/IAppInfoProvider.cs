using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Interface that represent a provider for app information.
    /// </summary>
    public interface IAppInfoProvider
    {
        /// <summary>
        /// Returns information related to a registered App.
        /// </summary>
        /// <param name="organizationId">The App's organization ID.</param>
        /// <param name="applicationId">The App's application ID.</param>
        /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the request fails to complete. See returned StatusCode for more details.</exception>
        /// <exception cref="UnauthorizedException">Thrown if authorization is missing for the request.</exception>
        /// <exception cref="ConnectionException">Thrown if a connection error is encountered for the request.</exception>
        /// <exception cref="ForbiddenException">Thrown if the caller is not authorized to make the request.</exception>
        /// <returns>The <see cref="AppInfo"/> associated to the provided <see cref="OrganizationId"/> and <see cref="AppId"/>.</returns>
        Task<AppInfo> GetAppInfoAsync(OrganizationId organizationId, AppId applicationId);

        /// <summary>
        /// Get list of all apps inside an organization.
        /// </summary>
        /// <remarks>
        /// Only apps that user has read access to will be returned.
        /// </remarks>
        /// <param name="organizationId">The App's organization ID.</param>
        /// <exception cref="System.Net.Http.HttpRequestException">This exception is thrown when the request fails to complete. See returned StatusCode for more details.</exception>
        /// <exception cref="UnauthorizedException">Thrown if authorization is missing for the request.</exception>
        /// <exception cref="ConnectionException">Thrown if a connection error is encountered for the request.</exception>
        /// <exception cref="ForbiddenException">Thrown if the caller is not authorized to make the request.</exception>
        /// <returns>The collection of <see cref="AppInfo"/> for an organization the user has read access to.</returns>
        Task<List<AppInfo>> GetAppsInfoAsync(OrganizationId organizationId);
    }
}
