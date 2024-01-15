using System;
using Unity.Cloud.AppLinking.Runtime;

namespace Unity.Cloud.AppLinking.Editor
{
    /// <summary>
    /// A static class that exposes the uri schemes used to register the app on a device.
    /// </summary>
    internal static class BuildUtils
    {
        /// <summary>
        /// Get the uri scheme uniquely identifying this app inside the namespace.
        /// </summary>
        /// <returns>
        /// The uri scheme uniquely identifying this app inside the namespace.
        /// </returns>
        public static string GetNamespacedUriScheme()
        {
            return UnityCloudPlayerSettings.Instance.GetAppNamespace();
        }
    }
}
