using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An interface that represents a provider of an app ID.
    /// </summary>
    public interface IAppIdProvider
    {
        /// <summary>
        /// Returns the App Id uniquely identifying an App on the cloud services.
        /// </summary>
        string GetAppId();
    }
}
