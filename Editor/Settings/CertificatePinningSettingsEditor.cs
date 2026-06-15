using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// Custom editor for <see cref="CertificatePinningSettings"/>.
    /// Draws the certificate pinning toggle and creates <c>Assets/CertificatePins/</c>
    /// when the feature is enabled for the first time.
    /// </summary>
    [CustomEditor(typeof(CertificatePinningSettings))]
    internal class CertificatePinningSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty m_IsEnabledProperty;

        void OnEnable()
        {
            m_IsEnabledProperty = serializedObject.FindProperty("m_IsEnabled");
        }

        /// <summary>
        /// Draws the settings GUI. Called by <see cref="CertificatePinningSettingsProvider"/>.
        /// </summary>
        public void DrawGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_IsEnabledProperty, new GUIContent("Enable Certificate Pinning"));

            var changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();

                if (m_IsEnabledProperty.boolValue)
                    EnsureCertificatePinsDirectoryExists();
                else
                    CertificatePinsManifestGenerator.DeleteManifestIfExists();
            }

            if (m_IsEnabledProperty.boolValue)
            {
                EditorGUILayout.HelpBox(
                    $"Certificate pinning is enabled. Place PEM public-key files under " +
                    $"'{CertificatePinsManifestGenerator.AssetsCertificatePinsRoot}/<hostname>/' to pin certificates for that host.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Certificate pinning is disabled. Enable it to create the CertificatePins directory and compile a pin manifest at build time.",
                    MessageType.None);
            }
        }

        static void EnsureCertificatePinsDirectoryExists()
        {
            var rootOnDisk = Path.Combine(Application.dataPath, "CertificatePins");

            if (Directory.Exists(rootOnDisk))
                return;

            Directory.CreateDirectory(rootOnDisk);
            AssetDatabase.Refresh();

            Debug.Log($"[CertificatePinning] Created directory '{CertificatePinsManifestGenerator.AssetsCertificatePinsRoot}'. " +
                      "Add a subfolder named after each hostname you want to pin, then place *.pem public-key files inside it.");
        }
    }
}
