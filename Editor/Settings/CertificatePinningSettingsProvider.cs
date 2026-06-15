using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// A <see cref="SettingsProvider"/> that exposes SSL certificate pinning options
    /// under <c>Project / Services / SSL Certificate</c>.
    /// </summary>
    internal class CertificatePinningSettingsProvider : SettingsProvider
    {
        const string k_AssetDirectory = "Assets/Unity Cloud/Editor/";

        CertificatePinningSettingsEditor m_Editor;

        CertificatePinningSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        /// <summary>
        /// Registers the settings provider with Unity's Project Settings window.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateCertificatePinningSettingsProvider()
        {
            return new CertificatePinningSettingsProvider("Project/Services/SSL Certificate", SettingsScope.Project)
            {
                keywords = new[]
                {
                    "certificate",
                    "pinning",
                    "ssl",
                    "tls",
                    "CertificatePins",
                }
            };
        }

        /// <inheritdoc/>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = AssetDatabase.LoadAssetAtPath<CertificatePinningSettings>(CertificatePinningSettings.k_AssetPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CertificatePinningSettings>();

                if (!AssetDatabase.IsValidFolder(k_AssetDirectory.TrimEnd('/')))
                {
                    Directory.CreateDirectory(k_AssetDirectory);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(settings, CertificatePinningSettings.k_AssetPath);
                AssetDatabase.SaveAssets();
            }

            m_Editor = UnityEditor.Editor.CreateEditor(settings) as CertificatePinningSettingsEditor;
        }

        /// <inheritdoc/>
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            m_Editor?.DrawGUI();
        }
    }
}
