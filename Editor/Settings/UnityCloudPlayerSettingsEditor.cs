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
        const string k_AppRegistrationMissingId = "Missing App Id";
        const string k_AppRegistrationWrongId = "Wrong App Id";
        const string k_AppRegistrationHelpInstruction = "Please enter the App Id assigned to this Unity Project in the Unity Cloud Portal and click the Refresh button to fetch its Name and Display Name values";

        const string k_DocumentationUriScheme = "https://";
        const string k_DocumentationLastestUrl = "docs.unity3d.com/Packages/com.unity.cloud.identity@latest/index.html";
        const string k_DocumentationGetStartedPageQueryArgument = "?subfolder=/manual/getting-started.html";

        SerializedProperty m_AppIdProperty;
        [ReadOnly] SerializedProperty m_AppNameProperty;
        [ReadOnly] SerializedProperty m_AppDisplayNameProperty;

        UnityHttpClient m_HttpClient;
        IAppInfoProvider m_AppInfoProvider;

        void OnEnable()
        {
            m_AppIdProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppId));
            m_AppNameProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppName));
            m_AppDisplayNameProperty = serializedObject.FindProperty(nameof(UnityCloudPlayerSettings.AppDisplayName));
        }

        public override void OnInspectorGUI()
        {
            DrawGUI();
        }

        public void DrawGUI()
        {
            serializedObject.Update();

            if (string.IsNullOrEmpty(m_AppIdProperty.stringValue))
            {
                EditorGUILayout.HelpBox(
                    $"{k_AppRegistrationMissingId}.\n{k_AppRegistrationHelpInstruction}.",
                    MessageType.Warning);
            }
            else
            {
                if (m_AppNameProperty.stringValue.Equals(UnityCloudPlayerSettings.k_DefaultAppName))
                {
                    EditorGUILayout.HelpBox(
                        $"{k_AppRegistrationWrongId}.\n{k_AppRegistrationHelpInstruction}.",
                        MessageType.Warning);
                }
            }

            if (GUILayout.Button("See manual for detailed information..."))
            {
                SynchronizationContext.Current.Send(_ =>
                {
                    Application.OpenURL($"{k_DocumentationUriScheme}{k_DocumentationLastestUrl}{k_DocumentationGetStartedPageQueryArgument}");
                }, null);
            }

            EditorGUILayout.PropertyField(m_AppIdProperty);

            if (GUILayout.Button("Refresh"))
            {
                SynchronizationContext.Current.Send(async _ =>
                {
                    await RefreshAppName();
                }, null);
            }

            EditorGUILayout.PropertyField(m_AppDisplayNameProperty);
            EditorGUILayout.PropertyField(m_AppNameProperty);
            serializedObject.ApplyModifiedProperties();
        }

        async Task RefreshAppName()
        {
            if (m_HttpClient == null)
            {
                var serviceHostResolver = UnityRuntimeServiceHostResolverFactory.Create();
                m_HttpClient = new UnityHttpClient();
                var serviceHttpClient = new ServiceHttpClient(m_HttpClient, null, UnityCloudPlayerSettings.Instance);
                m_AppInfoProvider = new AppInfoProvider(serviceHttpClient, serviceHostResolver);
            }

            if (m_AppIdProperty.stringValue.Length > 0)
            {
                try
                {
                    var m_AppInfo = await m_AppInfoProvider.GetAppInfoAsync(m_AppIdProperty.stringValue);
                    m_AppNameProperty.stringValue = m_AppInfo.Name;
                    m_AppDisplayNameProperty.stringValue = m_AppInfo.DisplayName;
                    serializedObject.ApplyModifiedProperties();

                    UnityCloudPlayerSettings.Instance.AppName = m_AppInfo.Name;
                    UnityCloudPlayerSettings.Instance.AppDisplayName = m_AppInfo.DisplayName;
                }
                catch (NotFoundException)
                {
                    Debug.LogWarning($"{k_AppRegistrationWrongId}.\n{k_AppRegistrationHelpInstruction}.");
                    UnityCloudPlayerSettings.Instance.AppName = UnityCloudPlayerSettings.k_DefaultAppName;
                    UnityCloudPlayerSettings.Instance.AppDisplayName = UnityCloudPlayerSettings.k_DefaultAppDisplayName;
                }
            }
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
