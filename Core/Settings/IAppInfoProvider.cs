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
        /// Gets an AppInfo from an app Id.
        /// </summary>
        /// <param name="id">The app id</param>
        /// <returns>An AppInfo instance</returns>
        Task<AppInfo> GetAppInfoAsync(string id);

        /// <summary>
        /// Gets the list of apps registered for an organization.
        /// </summary>
        /// <param name="userOrgId"></param>
        /// <returns>The list of apps registered for an organization.</returns>
        Task<List<AppInfo>> GetAppsInfoAsync(OrganizationId userOrgId);
    }
}
