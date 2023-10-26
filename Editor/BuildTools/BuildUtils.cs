using System;
using System.Linq;
using Unity.Cloud.Common.Runtime;
using UnityEditor.Build;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// A static class that exposes the uri schemes used to register the app on a device.
    /// </summary>
    public static class BuildUtils
    {
        const string k_AppRegistrationBuildExceptionMessageAppName = "Missing App Name value in Unity Cloud/App Registration player settings.";
        const string k_AppRegistrationBuildExceptionMessageAppId = "Missing App ID value in Unity Cloud/App Registration player settings.";
        const string k_AppRegistrationBuildExceptionMessageAppNamespace = "Missing App Namespace value in Unity Cloud/App Registration player settings.";
        const string k_DisableExceptionOnMissingAppNameCmdLineParameter = "-disableExceptionOnMissingAppName";

        static bool? s_ShouldThrowBuildFailedException = null;
        static bool ShouldThrowBuildFailedException
        {
            get
            {
                if (s_ShouldThrowBuildFailedException == null)
                {
                    s_ShouldThrowBuildFailedException = !Environment.GetCommandLineArgs()
                        .Select(arg => arg.Trim())
                        .Contains(k_DisableExceptionOnMissingAppNameCmdLineParameter);
                }

                return s_ShouldThrowBuildFailedException.Value;
            }
        }

        /// <summary>
        /// Get the custom uri scheme uniquely identifying this app.
        /// </summary>
        /// <returns>
        /// The custom uri scheme uniquely identifying this app.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if App Name value is default value.</exception>
        public static string GetCustomUriScheme()
        {
            if ((UnityCloudPlayerSettings.Instance.AppName.Equals(UnityCloudPlayerSettings.k_DefaultAppName)
                || string.IsNullOrEmpty(UnityCloudPlayerSettings.Instance.AppName)) &&
                ShouldThrowBuildFailedException)
            {
                throw new BuildFailedException(k_AppRegistrationBuildExceptionMessageAppName);
            }
            else if (string.IsNullOrEmpty(UnityCloudPlayerSettings.Instance.AppId) &&
                ShouldThrowBuildFailedException)
            {
                throw new BuildFailedException(k_AppRegistrationBuildExceptionMessageAppId);
            }
            return UnityCloudPlayerSettings.Instance.AppName;
        }

        /// <summary>
        /// Get the uri scheme uniquely identifying this app inside the namespace.
        /// </summary>
        /// <returns>
        /// The uri scheme uniquely identifying this app inside the namespace.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if App Name value is default value.</exception>
        public static string GetNamespacedUriScheme()
        {
            if ((UnityCloudPlayerSettings.Instance.AppName.Equals(UnityCloudPlayerSettings.k_DefaultAppName)
                || string.IsNullOrEmpty(UnityCloudPlayerSettings.Instance.AppName)) &&
                ShouldThrowBuildFailedException)
            {
                throw new BuildFailedException(k_AppRegistrationBuildExceptionMessageAppName);
            }
            if (string.IsNullOrEmpty(UnityCloudPlayerSettings.Instance.AppId) &&
                ShouldThrowBuildFailedException)
            {
                throw new BuildFailedException(k_AppRegistrationBuildExceptionMessageAppId);
            }
            if (string.IsNullOrEmpty(UnityCloudPlayerSettings.Instance.AppNamespace) &&
                ShouldThrowBuildFailedException)
            {
                throw new BuildFailedException(k_AppRegistrationBuildExceptionMessageAppNamespace);
            }
            return $"{UnityCloudPlayerSettings.Instance.GetAppNamespace()}.{UnityCloudPlayerSettings.Instance.AppName}";
        }
    }
}
