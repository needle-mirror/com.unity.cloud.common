using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;

#if USE_UC_IDENTITY
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Unity.Cloud.Identity;
using Unity.Cloud.Identity.Editor;
#endif

namespace Unity.Cloud.Common.Editor
{
    public class UnityCloudAppRegistration : UnityEditor.Editor
    {
        public IAppInfoProvider AppInfoProvider => m_AppInfoProvider;

        IAppInfoProvider m_AppInfoProvider;
        IServiceHttpClient m_ServiceHttpClient;
        IServiceHostResolver m_ServiceHostResolver;

        GUIStyle m_BorderStyle = null;
        GUIStyle m_SubHeadingStyle = null;

        const string k_DocumentationUriScheme = "https://";
        const string k_DocumentationLatestUrl = "docs.unity3d.com/Packages/com.unity.cloud.identity@latest/index.html";

#if USE_UC_IDENTITY

        IOrganizationRepository m_OrganizationRepository;
        IAuthenticator m_UnityEditorAuthenticator;

        List<AppInfo> m_Applications = new();
        List<IOrganization> m_Organizations = new();

        string m_NewAppName = string.Empty;
        string m_NewAppDisplayName = string.Empty;
        int m_NewAppOrganizationIndex = 0;
        string m_RefreshErrorMessage = string.Empty;
        string m_RegisterErrorMessage = string.Empty;

        int m_SelectedOrganizationIndex = 0;

        bool m_LoggedIn = false;
        bool m_RefreshErrorFlag = false;
        bool m_RegisterErrorFlag = false;

        [Serializable]
        struct AppRegistrationPayload
        {
            public string Name;
            public string DisplayName;
        }
#endif

        public delegate void SelectAppDelegate(OrganizationId orgId, AppId appId, string appName, string displayName);
        SelectAppDelegate m_SelectAppDelegate;

        void InitializeGUIStyles()
        {
            m_BorderStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(5, 5, 5, 5),
            };

            m_SubHeadingStyle = new GUIStyle(EditorStyles.boldLabel);
            m_SubHeadingStyle.fontSize = 16;
        }

        public async Task Initialize(SelectAppDelegate selectAppDelegate)
        {
            m_SelectAppDelegate = selectAppDelegate;

            m_ServiceHostResolver = UnityRuntimeServiceHostResolverFactory.Create();
            var httpClient = new UnityHttpClient();

#if USE_UC_IDENTITY
            var targetClientIdTokenToUnityServicesTokenExchanger =
                new TargetClientIdTokenToUnityServicesTokenExchanger(httpClient, m_ServiceHostResolver);
            m_UnityEditorAuthenticator = new UnityEditorAuthenticator(targetClientIdTokenToUnityServicesTokenExchanger);

            m_ServiceHttpClient = new ServiceHttpClient(httpClient, m_UnityEditorAuthenticator,
                UnityCloudPlayerSettings.Instance);
            m_AppInfoProvider = new AppInfoProvider(m_ServiceHttpClient, m_ServiceHostResolver);
            m_OrganizationRepository = new AuthenticatorOrganizationRepository(m_ServiceHttpClient, m_ServiceHostResolver);

            m_OrganizationRepository =
                new AuthenticatorOrganizationRepository(m_ServiceHttpClient, m_ServiceHostResolver);

            m_UnityEditorAuthenticator.AuthenticationStateChanged +=
                UnityEditorAuthenticator_AuthenticationStateChanged;
            await m_UnityEditorAuthenticator.InitializeAsync().ConfigureAwait(true);
#else
            m_ServiceHttpClient = new ServiceHttpClient(httpClient, null, UnityCloudPlayerSettings.Instance);
            m_AppInfoProvider = new AppInfoProvider(m_ServiceHttpClient, m_ServiceHostResolver);
#endif
        }

        public void DrawGUI()
        {
            if (m_BorderStyle == null || m_SubHeadingStyle == null)
            {
                InitializeGUIStyles();
            }

            GUILayout.Space(10);

#if USE_UC_IDENTITY
            if (m_LoggedIn)
            {
                GUILayout.Space(10);

                ShowExistingAppsUI();

                GUILayout.Space(10);

                RegisterNewAppUI();

                GUILayout.Space(20);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Please login to access existing apps or to register a new app. If you are unable to login, try restarting the editor.",
                    MessageType.Warning);
            }
#else
            EditorGUILayout.HelpBox("Install the com.unity.cloud.identity package to use more App Registration Functionality (Viewing/Editing existing applications and registering new applications).", MessageType.Info);

            if (GUILayout.Button("com.unity.cloud.identity Package Documentation"))
            {
                SynchronizationContext.Current.Send(_ =>
                {
                    Application.OpenURL($"{k_DocumentationUriScheme}{k_DocumentationLatestUrl}");
                }, null);
            }
#endif
            GUILayout.Space(10);
        }

#if USE_UC_IDENTITY
        void UnityEditorAuthenticator_AuthenticationStateChanged(AuthenticationState obj)
        {
            SynchronizationContext.Current.Send(async _ =>
            {
                m_LoggedIn = m_UnityEditorAuthenticator.AuthenticationState.Equals(AuthenticationState.LoggedIn);
                if (m_LoggedIn)
                {
                    if (await GetOrganizations())
                        await GetApplications();

                    Repaint();
                }
                else
                {
                    m_Organizations.Clear();
                    m_Applications.Clear();
                }
            }, null);
        }

        int SelectOrganizationDropdown(int currentSelectedIndex)
        {
            var newSelectedIndex = EditorGUILayout.Popup("Organization", currentSelectedIndex,
                m_Organizations.Select(item => item.Name).ToArray());

            return newSelectedIndex;
        }

        void SelectOrganization()
        {
            var newSelectedOrganizationIndex = SelectOrganizationDropdown(m_SelectedOrganizationIndex);
            if (newSelectedOrganizationIndex != m_SelectedOrganizationIndex)
            {
                m_SelectedOrganizationIndex = newSelectedOrganizationIndex;
                SynchronizationContext.Current.Send(async _ =>
                {
                    m_Applications.Clear();
                    await GetApplications();
                }, null);
            }
        }

        void RegisterNewAppUI()
        {
            EditorGUILayout.BeginVertical(m_BorderStyle);

            EditorGUILayout.LabelField("Register New App", m_SubHeadingStyle);

            EditorGUILayout.HelpBox(
                "App name should be globally unique, alphanumeric, lowercase, between 4-10 characters",
                MessageType.Info);
            m_NewAppName = EditorGUILayout.TextField("App Name:", m_NewAppName);
            m_NewAppDisplayName = EditorGUILayout.TextField("App Display Name:", m_NewAppDisplayName);
            m_NewAppOrganizationIndex = SelectOrganizationDropdown(m_NewAppOrganizationIndex);

            if (GUILayout.Button("Register New App"))
            {
                SynchronizationContext.Current.Send(async _ => { await RegisterNewApp(); }, null);
            }

            if (m_RegisterErrorFlag)
            {
                EditorGUILayout.HelpBox($"Error: {m_RegisterErrorMessage}", MessageType.Error);
            }

            EditorGUILayout.EndVertical();
        }

        void ShowExistingAppsUI()
        {
            EditorGUILayout.BeginVertical(m_BorderStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Registered Applications", m_SubHeadingStyle);

            var refreshButton = new GUIContent(EditorGUIUtility.IconContent("d_RotateTool").image, "Click to Refresh");
            if (GUILayout.Button(refreshButton, GUILayout.Width(50)))
            {
                SynchronizationContext.Current.Send(async _ => { await GetApplications(); }, null);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            SelectOrganization();

            if (m_RefreshErrorFlag)
            {
                EditorGUILayout.HelpBox($"Error: {m_RefreshErrorMessage}", MessageType.Error);
            }

            if (m_Organizations.Count > 0)
            {
                EditorGUILayout.BeginVertical(m_BorderStyle);

                var selectedOrgId = m_Organizations[m_SelectedOrganizationIndex].Id;

                foreach (var app in m_Applications)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();

                    GenerateField("App Name", app.Name);
                    GenerateField("App Display Name", app.DisplayName);
                    GenerateField("App ID", app.Id.ToString());

                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("Select", GUILayout.Width(80), GUILayout.Height(60)))
                    {
                        m_SelectAppDelegate(selectedOrgId, app.Id, app.Name, app.DisplayName);
                    }

                    EditorGUILayout.BeginVertical();

                    if (GUILayout.Button("Edit", GUILayout.Width(80), GUILayout.Height(40)))
                    {
                        EditApp(app.Id, app.Name, app.DisplayName);
                        return;
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(80), GUILayout.Height(20)))
                    {
                        DeleteApp(app.Id, app.Name, app.DisplayName);
                        return;
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndVertical();
            }


            EditorGUILayout.EndVertical();
        }

        static void GenerateField(string fieldLabel, string fieldValue, float fieldLabelWidth = 200,
            bool showCopyButton = true)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField(fieldLabel, GUILayout.Width(fieldLabelWidth));
            EditorGUILayout.TextField(fieldValue);
            EditorGUI.EndDisabledGroup();

            if (showCopyButton)
            {
                if (GUILayout.Button("Copy", GUILayout.Width(60)))
                {
                    EditorGUIUtility.systemCopyBuffer = fieldValue;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        async Task GetApplications()
        {
            if (m_Organizations.Count == 0)
            {
                Debug.LogWarning("No organizations found.");
                return;
            }

            try
            {
                m_Applications = await m_AppInfoProvider.GetAppsInfoAsync(m_Organizations[m_SelectedOrganizationIndex].Id);
                m_RefreshErrorFlag = false;
            }
            catch (ServiceException ex)
            {
                m_RefreshErrorFlag = true;
                if (ex is UnauthorizedException)
                {
                    m_RefreshErrorMessage = "You are not logged in. You might need to restart the editor";
                }
                else if (ex is ForbiddenException)
                {
                    m_RefreshErrorMessage = "Invalid Organization ID.";
                }

                Debug.LogException(ex);
                m_Applications.Clear();
            }
        }

        async Task<bool> GetOrganizations()
        {
            try
            {
                var organizations = await m_OrganizationRepository.ListOrganizationsAsync();
                m_Organizations = organizations.ToList();
                m_SelectedOrganizationIndex = 0;
                return true;
            }
            catch (ServiceException ex)
            {
                if (ex is UnauthorizedException)
                {
                    // Assume that authentication is still happening
                    return false;
                }
                else
                {
                    Debug.LogException(ex);
                    return false;
                }
            }
        }

        async Task RegisterNewApp()
        {
            try
            {
                var orgId = m_Organizations[m_NewAppOrganizationIndex].Id;
                var registerUri = m_ServiceHostResolver.GetResolvedRequestUri($"/app-linking/v1/organizations/{orgId}/applications");
                var registerPayload = new AppRegistrationPayload
                {
                    Name = m_NewAppName,
                    DisplayName = m_NewAppDisplayName
                };
                var newAppInfo = await m_ServiceHttpClient.PostJsonAsync<AppInfo>(registerUri, registerPayload);

                m_NewAppName = String.Empty;
                m_NewAppDisplayName = String.Empty;

                m_SelectAppDelegate(orgId, newAppInfo.Id, newAppInfo.Name, newAppInfo.DisplayName);

                m_RegisterErrorFlag = false;

                await GetApplications();
            }
            catch (ServiceException ex)
            {
                m_RegisterErrorFlag = true;
                m_RegisterErrorMessage = ex.Message;
                Debug.LogException(ex);
            }
        }

        void EditApp(AppId appId, string appName, string displayName)
        {
            var editWindow = CreateInstance<EditApplicationPopup>();
            editWindow.Initialize(this, appName, displayName, appId, m_Organizations[m_SelectedOrganizationIndex].Id);
            var cursorPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            editWindow.position = new Rect(cursorPosition.x - 300, cursorPosition.y - 100, 600, 200);
            editWindow.ShowModal();
        }

        void DeleteApp(AppId appId, string appName, string displayName)
        {
            var deleteWindow = CreateInstance<DeleteApplicationPopup>();
            deleteWindow.Initialize(this, appName, displayName, appId, m_Organizations[m_SelectedOrganizationIndex].Id);
            var cursorPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            deleteWindow.position = new Rect(cursorPosition.x - 300, cursorPosition.y - 100, 600, 200);
            deleteWindow.ShowModal();
        }

        class EditApplicationPopup : EditorWindow
        {
            UnityCloudAppRegistration m_AppRegistration;

            string m_OriginalAppName = string.Empty;
            string m_OriginalAppDisplayName = string.Empty;
            string m_NewAppName = string.Empty;
            string m_NewAppDisplayName = string.Empty;

            OrganizationId m_OrgId = OrganizationId.None;
            AppId m_AppId = AppId.None;

            struct ErrorMessage
            {
                public string Message;
                public string ErrorCode;
            }

            public void Initialize(UnityCloudAppRegistration appRegistration, string originalAppName, string originalAppDisplayName, AppId appId, OrganizationId orgId)
            {
                m_AppRegistration = appRegistration;
                m_OriginalAppName = originalAppName;
                m_OriginalAppDisplayName = originalAppDisplayName;
                m_AppId = appId;
                m_OrgId = orgId;

                m_NewAppName = m_OriginalAppName;
                m_NewAppDisplayName = m_OriginalAppDisplayName;
            }

            void OnGUI()
            {
                EditorGUILayout.Space(10);

                GUILayout.Label("Edit Application", EditorStyles.boldLabel);

                EditorGUILayout.Space(10);

                GenerateField("Current Application Name", m_OriginalAppName, 300);
                GenerateField("Current Application Display Name", m_OriginalAppDisplayName, 300);

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("New Application Name: ", GUILayout.Width(200));
                m_NewAppName = EditorGUILayout.TextField(m_NewAppName);
                GUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("New Application Display Name: ", GUILayout.Width(200));
                m_NewAppDisplayName = EditorGUILayout.TextField(m_NewAppDisplayName);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Update Application"))
                {
                    SynchronizationContext.Current.Send(async _ => { await EditApplication(); }, null);
                }

                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
            }

            async Task EditApplication()
            {
                try
                {
                    var registerUri = m_AppRegistration.m_ServiceHostResolver.GetResolvedRequestUri($"/app-linking/v1/organizations/{m_OrgId}/applications/{m_AppId}");
                    var updatePayload = new AppRegistrationPayload
                    {
                        Name = m_NewAppName,
                        DisplayName = m_NewAppDisplayName
                    };

                    Close();

                    var httpContent = new StringContent(JsonSerialization.Serialize(updatePayload), Encoding.UTF8,
                        "application/json");
                    var response = await m_AppRegistration.m_ServiceHttpClient.PutAsync(registerUri, httpContent);

                    var updatedAppInfo = await response.JsonDeserializeAsync<AppInfo>();

                    // If a valid AppInfo object is not returned, it is assumed an error occurred
                    if (updatedAppInfo.Id == null || updatedAppInfo.Name == null)
                    {
                        var errorMessage = await response.JsonDeserializeAsync<ErrorMessage>();
                        EditorUtility.DisplayDialog("App Registration Error",
                            $"An error occured when editing the application: {errorMessage.Message}", "Ok", "");
                        throw new Exception(errorMessage.Message);
                    }

                    if (updatedAppInfo.Id == UnityCloudPlayerSettings.Instance.GetAppId())
                    {
                        m_AppRegistration.m_SelectAppDelegate(m_OrgId, updatedAppInfo.Id, updatedAppInfo.Name,
                            updatedAppInfo.DisplayName);
                    }

                    m_AppRegistration.m_Applications.Clear();
                    await m_AppRegistration.GetApplications();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        class DeleteApplicationPopup : EditorWindow
        {
            UnityCloudAppRegistration m_AppRegistration;

            AppId m_AppId = AppId.None;

            OrganizationId m_OrgId = OrganizationId.None;
            string m_AppName = string.Empty;
            string m_AppDisplayName = string.Empty;

            public void Initialize(UnityCloudAppRegistration appRegistration, string appName, string appDisplayName, AppId appId, OrganizationId orgId)
            {
                m_AppRegistration = appRegistration;
                m_AppId = appId;
                m_OrgId = orgId;
                m_AppName = appName;
                m_AppDisplayName = appDisplayName;
            }

            void OnGUI()
            {
                EditorGUILayout.Space(10);

                GUILayout.Label("Delete Application", EditorStyles.boldLabel);

                EditorGUILayout.Space(10);

                GenerateField("Application Name", m_AppName, showCopyButton: false);
                GenerateField("Application Display Name", m_AppDisplayName, showCopyButton: false);
                GenerateField("Application ID", m_AppId.ToString(), showCopyButton: false);

                EditorGUILayout.Space(10);

                GUILayout.Label(
                    "Are you sure you want to delete this application? Once deleted, the application can not be recovered.",
                    EditorStyles.wordWrappedLabel);

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Delete Application"))
                {
                    SynchronizationContext.Current.Send(async _ => { await DeleteApplication(); }, null);
                }

                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
            }

            async Task DeleteApplication()
            {
                try
                {
                    Close();

                    var deleteUri = m_AppRegistration.m_ServiceHostResolver.GetResolvedRequestUri($"/app-linking/v1/organizations/{m_OrgId}/applications/{m_AppId}");
                    await m_AppRegistration.m_ServiceHttpClient.DeleteAsync(deleteUri);

                    if (m_AppId == UnityCloudPlayerSettings.Instance.GetAppId())
                    {
                        m_AppRegistration.m_SelectAppDelegate(m_OrgId, new AppId(String.Empty), String.Empty, String.Empty);
                    }

                    m_AppRegistration.m_Applications.Clear();
                    await m_AppRegistration.GetApplications();
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog("App Registration Error",
                        $"An error occured when deleting the application: {ex.Message}", "Ok", "");
                    Debug.LogException(ex);
                }
            }
        }
#endif
    }
}
