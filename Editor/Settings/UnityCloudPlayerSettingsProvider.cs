using System.IO;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Common.Editor
{
    public class UnityCloudPlayerSettingsProvider : SettingsProvider
    {
        const string k_ResourcesDirectory = "Assets/Unity Cloud/Resources/";
        static readonly string k_AssetPath = $"{k_ResourcesDirectory}{UnityCloudPlayerSettings.k_AssetName}.asset";

        UnityCloudPlayerSettingsEditor m_UnityCloudPlayerSettingsEditor;

        UnityCloudPlayerSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) {}

        [SettingsProvider]
        public static SettingsProvider CreateUnityCloudPlayerSettingsProvider()
        {
            return new UnityCloudPlayerSettingsProvider("Project/Unity Cloud/App Registration", SettingsScope.Project)
            {
                keywords = new[]
                {
                    nameof(UnityCloudPlayerSettings.AppId),
                }
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = Resources.Load<UnityCloudPlayerSettings>(UnityCloudPlayerSettings.k_AssetName);

            if (settings == null)
            {
                settings = UnityCloudPlayerSettings.Instance;

                if (!AssetDatabase.IsValidFolder(k_ResourcesDirectory))
                    Directory.CreateDirectory(k_ResourcesDirectory);

                AssetDatabase.CreateAsset(settings, k_AssetPath);
                AssetDatabase.SaveAssets();
            }

            m_UnityCloudPlayerSettingsEditor = UnityEditor.Editor.CreateEditor(settings) as UnityCloudPlayerSettingsEditor;
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            m_UnityCloudPlayerSettingsEditor.DrawGUI();
        }
    }
}
