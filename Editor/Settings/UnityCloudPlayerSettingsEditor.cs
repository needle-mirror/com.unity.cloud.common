using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    [CustomEditor(typeof(UnityCloudPlayerSettings))]
    public class UnityCloudPlayerSettingsEditor : UnityEditor.Editor
    {
        const string k_DocumentationUriScheme = "https://";
        const string k_DocumentationLastestUrl = "docs.unity3d.com/Packages/com.unity.cloud.identity@latest/index.html";
        const string k_DocumentationGetStartedPageQueryArgument = "?subfolder=/manual/getting-started.html";

        [ReadOnly] SerializedProperty m_AppIdProperty;
        [ReadOnly] SerializedProperty m_AppNameProperty;
        [ReadOnly] SerializedProperty m_AppDisplayNameProperty;
        
        string m_ManualEntryAppId = string.Empty;
        string m_ManualEntryErrorMessage = string.Empty;

        bool m_ManualEntryErrorFlag = false;

        UnityCloudAppRegistration m_AppRegistration;

        void OnEnable()
        {
            m_AppIdProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppId));
            m_AppNameProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppName));
            m_AppDisplayNameProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppDisplayName));
        }

        async Task Awake()
        {
            m_AppRegistration = CreateInstance<UnityCloudAppRegistration>();
            await m_AppRegistration.Initialize(SelectApp);
        }

        public override void OnInspectorGUI()
        {
            DrawGUI();
        }

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
                    Application.OpenURL($"{k_DocumentationUriScheme}{k_DocumentationLastestUrl}{k_DocumentationGetStartedPageQueryArgument}");
                }, null);
            }
        }

        void ManualAppEntry()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enter Application ID: ", GUILayout.Width(200));
            m_ManualEntryAppId = EditorGUILayout.TextField(m_ManualEntryAppId);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                SynchronizationContext.Current.Send(async _ =>
                {
                    await GetAppInformation();
                }, null);
            }
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
                var appInfo = await m_AppRegistration.m_AppInfoProvider.GetAppInfoAsync(m_ManualEntryAppId);
                SelectApp(appInfo.Id, appInfo.Name, appInfo.DisplayName);
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
        }

        void SelectApp(string appId, string appName, string displayName)
        {
            m_AppIdProperty.stringValue = appId;
            m_AppNameProperty.stringValue = appName;
            m_AppDisplayNameProperty.stringValue = displayName;

            UnityCloudPlayerSettings.Instance.AppId = appId;
            UnityCloudPlayerSettings.Instance.AppName = appName;
            UnityCloudPlayerSettings.Instance.AppDisplayName = displayName;
        }
    }


    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
