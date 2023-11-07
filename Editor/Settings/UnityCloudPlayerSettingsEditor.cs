using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// A custom editor for <see cref="UnityCloudPlayerSettings"/>.
    /// </summary>
    [CustomEditor(typeof(UnityCloudPlayerSettings))]
    public class UnityCloudPlayerSettingsEditor : UnityEditor.Editor
    {
        const string k_DocumentationUriScheme = "https://";
        const string k_DocumentationLatestUrl = "docs.unity3d.com/Packages/com.unity.cloud.identity@latest/index.html";
        const string k_DocumentationGetStartedPageQueryArgument = "?subfolder=/manual/getting-started.html";

        [ReadOnly] SerializedProperty m_AppIdProperty;
        [ReadOnly] SerializedProperty m_AppNameProperty;
        [ReadOnly] SerializedProperty m_AppDisplayNameProperty;
        [ReadOnly] SerializedProperty m_AppOrganizationIdProperty;
        // Namespace is not tied to app registration information
        SerializedProperty m_AppNamespaceProperty;

        string m_ManualEntryOrgId = string.Empty;
        string m_ManualEntryAppId = string.Empty;
        string m_ManualEntryErrorMessage = string.Empty;

        bool m_ManualEntryErrorFlag = false;

        UnityCloudAppRegistration m_AppRegistration;

        void OnEnable()
        {
            m_AppIdProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppId));
            m_AppNameProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppName));
            m_AppDisplayNameProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppDisplayName));
            m_AppOrganizationIdProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppOrganizationID));
            m_AppNamespaceProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppNamespace));
        }

        async Task Awake()
        {
            EditorUtility.SetDirty(UnityCloudPlayerSettings.Instance);
            m_AppRegistration = CreateInstance<UnityCloudAppRegistration>();
            await m_AppRegistration.Initialize(SelectApp);
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            DrawGUI();
        }

        /// <summary>
        /// Draws the Editor GUI.
        /// </summary>
        public void DrawGUI()
        {
            serializedObject.Update();

            ShowManualUI();

            ShowCurrentCloudPlayerSettingsUI();

            GUILayout.Space(10);

            ManualAppEntry();

            m_AppRegistration.DrawGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void ShowManualUI()
        {
            if (GUILayout.Button("See manual for detailed information..."))
            {
                SynchronizationContext.Current.Send(_ =>
                {
                    Application.OpenURL($"{k_DocumentationUriScheme}{k_DocumentationLatestUrl}{k_DocumentationGetStartedPageQueryArgument}");
                }, null);
            }
        }

        void ManualAppEntry()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Enter Organization ID: ", GUILayout.Width(200));
            m_ManualEntryOrgId = EditorGUILayout.TextField(m_ManualEntryOrgId);
            EditorGUILayout.LabelField("Enter Application ID: ", GUILayout.Width(200));
            m_ManualEntryAppId = EditorGUILayout.TextField(m_ManualEntryAppId);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                SynchronizationContext.Current.Send(async _ =>
                {
                    await GetAppInformation();
                }, null);
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (m_ManualEntryErrorFlag)
            {
                EditorGUILayout.HelpBox($"Error: {m_ManualEntryErrorMessage}", MessageType.Error);
            }
        }

        async Task GetAppInformation()
        {
            try
            {
                var orgId = new OrganizationId(m_ManualEntryOrgId);
                var appId = new AppId(m_ManualEntryAppId);

                var appInfo = await m_AppRegistration.AppInfoProvider.GetAppInfoAsync(orgId, appId);
                SelectApp(orgId, appInfo.Id, appInfo.Name, appInfo.DisplayName);
                m_ManualEntryErrorFlag = false;
            }
            catch (NotFoundException)
            {
                m_ManualEntryErrorFlag = true;
                m_ManualEntryErrorMessage = "Application ID Not Found";
            }
        }

        void ShowCurrentCloudPlayerSettingsUI()
        {
            EditorGUILayout.PropertyField(m_AppNameProperty);
            EditorGUILayout.PropertyField(m_AppDisplayNameProperty);
            EditorGUILayout.PropertyField(m_AppIdProperty);
            EditorGUILayout.PropertyField(m_AppOrganizationIdProperty);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_AppNamespaceProperty);
            if (EditorGUI.EndChangeCheck())
            {
                var newValue = SanitizeNamespace(m_AppNamespaceProperty.stringValue);
                if (!newValue.Equals(m_AppNamespaceProperty.stringValue))
                {
                    m_AppNamespaceProperty.stringValue = newValue;
                    UnityCloudPlayerSettings.Instance.AppNamespace = newValue;
                }
            }
        }

        string SanitizeNamespace(string stringValue)
        {
            stringValue = Regex.Replace(stringValue, "\\s+", "", RegexOptions.None, TimeSpan.FromSeconds(1));
            if (stringValue[^1].Equals('.'))
            {
                stringValue = stringValue.TrimEnd('.');
            }
            return stringValue;
        }

        void SelectApp(OrganizationId orgId, AppId appId, string appName, string displayName)
        {
            m_AppIdProperty.stringValue = appId.ToString();
            m_AppNameProperty.stringValue = appName;
            m_AppDisplayNameProperty.stringValue = displayName;
            m_AppOrganizationIdProperty.stringValue = orgId.ToString();

            UnityCloudPlayerSettings.Instance.AppId = appId.ToString();
            UnityCloudPlayerSettings.Instance.AppName = appName;
            UnityCloudPlayerSettings.Instance.AppDisplayName = displayName;
            UnityCloudPlayerSettings.Instance.AppOrganizationID = orgId.ToString();
        }
    }


    /// <summary>
    /// Draws a property as read-only in the GUI.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
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
