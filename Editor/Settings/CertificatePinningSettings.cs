using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// Project-level settings for SSL certificate pinning.
    /// Persisted as a ScriptableObject under <c>Assets/Unity Cloud/Resources/</c>.
    /// </summary>
    internal class CertificatePinningSettings : ScriptableObject
    {
        internal const string k_AssetName = "CertificatePinningSettings";
        internal const string k_AssetPath = "Assets/Unity Cloud/Editor/CertificatePinningSettings.asset";

        [SerializeField]
        bool m_IsEnabled = false;

        /// <summary>
        /// Whether certificate pinning is enabled for this project.
        /// When enabled, the <c>Assets/CertificatePins/</c> directory is created and
        /// PEM files placed inside it are compiled into the pin manifest at build time.
        /// </summary>
        internal bool Enabled
        {
            get => m_IsEnabled;
            set => m_IsEnabled = value;
        }

        /// <summary>
        /// Loads the settings asset and returns whether certificate pinning is enabled.
        /// Returns <c>false</c> if the settings asset has not been created yet.
        /// </summary>
        internal static bool IsEnabled()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CertificatePinningSettings>(k_AssetPath);
            return settings != null && settings.m_IsEnabled;
        }
    }
}
