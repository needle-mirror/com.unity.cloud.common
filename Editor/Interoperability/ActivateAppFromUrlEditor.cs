using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
   [CustomEditor(typeof(ActivateAppFromUrl))]
   public class ActivateAppFromUrlEditor : UnityEditor.Editor
    {
        SerializedProperty m_ActivationUrlProperty;
        SerializedProperty m_ActivateAtStartUpProperty;
        IUrlRedirectionInterceptor UrlRedirectionInterceptor;

        void OnEnable()
        {
            UrlRedirectionInterceptor = Runtime.UrlRedirectionInterceptor.GetInstance();

            m_ActivationUrlProperty = serializedObject.FindProperty(nameof(ActivateAppFromUrl.m_ActivationUrl));
            m_ActivateAtStartUpProperty = serializedObject.FindProperty(nameof(ActivateAppFromUrl.m_ActivateAtStartUp));
        }

        public override void OnInspectorGUI()
        {
            DrawGUI();
        }

        public void DrawGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_ActivationUrlProperty);
            EditorGUILayout.PropertyField(m_ActivateAtStartUpProperty);

            GUI.enabled = !m_ActivateAtStartUpProperty.boolValue && !string.IsNullOrEmpty(m_ActivationUrlProperty.stringValue) && Application.isPlaying;

            if (GUILayout.Button("Activate"))
            {
                UrlRedirectionInterceptor.InterceptAwaitedUrl(m_ActivationUrlProperty.stringValue);
            }

            serializedObject.ApplyModifiedProperties();
        }
   }
}
