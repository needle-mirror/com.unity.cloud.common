using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An interface that represents a provider for an app namespace.
    /// </summary>
    public interface IAppNamespaceProvider
    {
        /// <summary>
        /// Returns the App namespace uniquely identifying an App on a device.
        /// </summary>
        string GetAppNamespace();
    }
}
