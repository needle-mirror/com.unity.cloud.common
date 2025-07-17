using System;
using System.Text.RegularExpressions;
using Unity.Cloud.AppLinking.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.AppLinking.Editor
{
    /// <summary>
    /// A custom editor for <see cref="UnityCloudPlayerSettings"/>.
    /// </summary>
    [CustomEditor(typeof(UnityCloudPlayerSettings))]
    internal class UnityCloudPlayerSettingsEditor : UnityEditor.Editor
    {

        [ReadOnly] SerializedProperty m_AppIdProperty;
        SerializedProperty m_AppNamespaceProperty;

        void OnEnable()
        {
            m_AppIdProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppId));
            m_AppNamespaceProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppNamespace));
        }

        void Awake()
        {
            EditorUtility.SetDirty(UnityCloudPlayerSettings.Instance);
        }

        /// <summary>
        /// Draws the Editor GUI.
        /// </summary>
        public void DrawGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            ShowCurrentCloudPlayerSettingsUI();

            var hasChanged = EditorGUI.EndChangeCheck();
            if (hasChanged)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        void ShowCurrentCloudPlayerSettingsUI()
        {
            if (m_AppIdProperty.stringValue != CloudProjectSettings.projectId)
            {
                m_AppIdProperty.stringValue = CloudProjectSettings.projectId;
                UnityCloudPlayerSettings.Instance.AppId = CloudProjectSettings.projectId;
            }

            var infoMessage = "";
            var namespaceIsValid = false;
            if (m_AppNamespaceProperty.stringValue.Equals(UnityCloudPlayerSettings.s_DefaultAppNamespace))
            {
                infoMessage =
                    "Please change the 'default' value for a unique App Namespace to enable activation of your standalone build from URL.";
                EditorGUILayout.HelpBox(infoMessage, MessageType.Warning);
            }
            else
            {
                var safeNamespaceValue = SanitizeNamespace(m_AppNamespaceProperty.stringValue);
                namespaceIsValid = safeNamespaceValue.Equals(m_AppNamespaceProperty.stringValue);
                if (!namespaceIsValid)
                {
                    infoMessage =
                        "This App Namespace is invalid. Please click the 'Validate Namespace' button to generate a valid App Namespace value.";
                    EditorGUILayout.HelpBox(infoMessage, MessageType.Warning);
                }
                else
                {
                    infoMessage =
                        $"Your standalone build can be activated from URLs starting with {m_AppNamespaceProperty.stringValue}://*";
                    EditorGUILayout.HelpBox(infoMessage, MessageType.Info);
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(m_AppNamespaceProperty);

            if (GUILayout.Button(namespaceIsValid ? "Apply Change" : "Validate Namespace"))
            {
                var newValue = SanitizeNamespace(m_AppNamespaceProperty.stringValue);
                if (!newValue.Equals(m_AppNamespaceProperty.stringValue))
                {
                    m_AppNamespaceProperty.stringValue = newValue;
                    UnityCloudPlayerSettings.Instance.AppNamespace = newValue;
                }

                // Move focus out of PropertyField to apply change
                GUI.FocusControl(null);
            }
        }

        string SanitizeNamespace(string stringValue)
        {
            if (stringValue.Length == 0)
            {
                return UnityCloudPlayerSettings.s_DefaultAppNamespace;
            }
            // Ensure casing is invariant for url transport
            stringValue = stringValue.ToLowerInvariant();
            // Remove unsupported characters in a scheme
            stringValue = Regex.Replace(stringValue, @"[^a-zA-Z0-9\+\.\-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
            // Replace any multiple "." with a single "."
            stringValue = Regex.Replace(stringValue, @"\.{2,}", ".", RegexOptions.None, TimeSpan.FromSeconds(1));
            // Ensure first character is not a "."
            if (stringValue.Length > 0 && stringValue[0].Equals('.'))
            {
                stringValue = stringValue.TrimStart('.');
            }
            // Ensure first character is a letter by prepending "default." expression
            if (stringValue.Length > 0 && !Regex.IsMatch(stringValue, @"^[a-zA-Z]"))
            {
                stringValue = $"replace-me.{stringValue}";
            }
            // Ensure end character is not a "."
            if (stringValue.Length > 0 && stringValue[^1].Equals('.'))
            {
                stringValue = stringValue.TrimEnd('.');
            }
            // If we end up with an empty string
            if (stringValue.Length == 0)
            {
                stringValue = UnityCloudPlayerSettings.s_DefaultAppNamespace;
            }
            return stringValue;
        }
    }

    /// <summary>
    /// Draws a property as read-only in the GUI.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Gets the height of the property.
        /// </summary>
        /// <param name="property">The property to draw.</param>
        /// <param name="label">The content label.</param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
