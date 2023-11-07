using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An interface that represents a provider for an app display name.
    /// </summary>
    public interface IAppDisplayNameProvider
    {
        /// <summary>
        /// Returns the App display name.
        /// </summary>
        /// <returns>The App display name.</returns>
        string GetAppDisplayName();
    }
}
