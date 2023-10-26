using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A class which provides the default App namespace.
    /// </summary>
    public class DefaultAppNamespaceProvider : IAppNamespaceProvider
    {
        /// <inheritdoc/>
        public string GetAppNamespace()
        {
            return "com.unity.cloud";
        }
    }
}
