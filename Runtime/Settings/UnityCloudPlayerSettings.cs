using System;
using UnityEngine;

    namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// A class containing Unity Player Settings for a Unity Cloud app.
    /// </summary>
    public class UnityCloudPlayerSettings : ScriptableObject, IAppIdProvider, IAppNameProvider, IAppDisplayNameProvider, IAppNamespaceProvider
    {
        /// <summary>
        /// The asset name for the <see cref="UnityCloudPlayerSettings"/> scriptable object.
        /// </summary>
        public const string k_AssetName = "UnityCloudPlayerSettings";

        /// <summary>
        /// The default app name.
        /// </summary>
        public const string k_DefaultAppName = "default";

        /// <summary>
        /// The default app display name.
        /// </summary>
        public const string k_DefaultAppDisplayName = "Default";

        /// <summary>
        /// The default app display name.
        /// </summary>
        public const string k_DefaultOrganizationID = "Default";

        /// <summary>
        /// The default app namespace.
        /// </summary>
        public const string k_DefaultNamespace = "com.unity.cloud";

#pragma warning disable S1104 // Make this field 'private' and encapsulate it in a 'public' property.

        /// <summary>
        /// The Unity Cloud app ID.
        /// </summary>
        [ReadOnly] public string AppId;

        /// <summary>
        /// The Unity Cloud app name.
        /// </summary>
        [ReadOnly] public string AppName = k_DefaultAppName;

        /// <summary>
        /// The Unity Cloud app display name.
        /// </summary>
        [ReadOnly] public string AppDisplayName = k_DefaultAppDisplayName;

        /// <summary>
        /// The Unity Cloud app organization ID.
        /// </summary>
        [ReadOnly] public string AppOrganizationID = k_DefaultOrganizationID;

        /// The Unity Cloud namespace to uniquely identify the app on the device.
        /// </summary>
        public string AppNamespace = k_DefaultNamespace;

#pragma warning restore S1104

        static UnityCloudPlayerSettings s_Instance;

        /// <summary>
        /// Gets or creates the current instance of the <see cref="UnityCloudPlayerSettings"/>.
        /// </summary>
        public static UnityCloudPlayerSettings Instance
        {
            get
            {
                UnitySynchronizationContextGrabber.s_UnitySynchronizationContext.Send(_ =>
                {
                    if (s_Instance == null)
                    {
                        s_Instance = Resources.Load<UnityCloudPlayerSettings>(k_AssetName);

                        if (s_Instance == null)
                        {
                            s_Instance = CreateInstance<UnityCloudPlayerSettings>();
                        }
                    }
                }, null);

                return s_Instance;
            }
        }

        /// <summary>
        /// Gets the app ID.
        /// </summary>
        /// <returns>The app ID.</returns>
        public AppId GetAppId()
        {
            return new AppId(Instance.AppId);
        }

        /// <summary>
        /// Gets the app name.
        /// </summary>
        /// <returns>The app name.</returns>
        public string GetAppName()
        {
            return Instance.AppName;
        }

        /// <summary>
        /// Gets the app display name.
        /// </summary>
        /// <returns>The app display name.</returns>
        public string GetAppDisplayName()
        {
            return Instance.AppDisplayName;
        }

        /// <summary>
        /// Gets the app organization ID.
        /// </summary>
        /// <returns>The app organization ID.</returns>
        public string GetAppOrganization()
        {
            return Instance.AppOrganizationID;
        }

        /// <summary>
        /// Gets the app name namespace.
        /// </summary>
        /// <returns>The app namespace.</returns>
        public string GetAppNamespace()
        {
            return Instance.AppNamespace;
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}
