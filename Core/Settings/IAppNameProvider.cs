using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An interface that represents a provider for an app name.
    /// </summary>
    public interface IAppNameProvider
    {
        /// <summary>
        /// Returns the App Name uniquely identifying an App on the cloud services.
        /// </summary>
        /// <returns>The App Name.</returns>
        string GetAppName();
    }
}
