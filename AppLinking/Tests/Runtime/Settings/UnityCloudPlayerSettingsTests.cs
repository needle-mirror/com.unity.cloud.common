using NUnit.Framework;
using Unity.Cloud.AppLinking.Runtime;
using UnityEngine;

namespace Unity.Cloud.AppLinking.Tests
{
    public class UnityCloudPlayerSettingsTests
    {
        string m_ExpectedAppId;
        string m_ExpectedAppName;
        string m_ExpectedAppDisplayName;
        string m_ExpectedAppOrganizationId;
        string m_ExpectedAppNamespace;

        [SetUp]
        public void Test()
        {
            var settings = Resources.Load<UnityCloudPlayerSettings>(UnityCloudPlayerSettings.k_AssetName);

            if (settings != null)
            {
                m_ExpectedAppId = settings.AppId;
                m_ExpectedAppName = settings.AppName;
                m_ExpectedAppDisplayName = settings.AppDisplayName;
                m_ExpectedAppOrganizationId = settings.AppOrganizationID;
                m_ExpectedAppNamespace = settings.AppNamespace;
            }
            else
            {
                m_ExpectedAppId = string.Empty;
                m_ExpectedAppName = UnityCloudPlayerSettings.s_DefaultAppName;
                m_ExpectedAppDisplayName = UnityCloudPlayerSettings.s_DefaultAppDisplayName;
                m_ExpectedAppOrganizationId = UnityCloudPlayerSettings.s_DefaultOrganizationID;
                m_ExpectedAppNamespace = UnityCloudPlayerSettings.s_DefaultAppNamespace;
            }
        }

        [Test]
        public void GetAppId_NoInput_CorrectAppId()
        {
            // Arrange
            var instance = UnityCloudPlayerSettings.Instance;

            // Act
            var appId = instance.GetAppId();

            // Assert
            Assert.AreEqual(m_ExpectedAppId, appId.ToString());
        }

        [Test]
        public void GetAppName_NoInput_CorrectAppName()
        {
            // Arrange
            var instance = UnityCloudPlayerSettings.Instance;

            // Act
            var appName = instance.AppName;

            // Assert
            Assert.AreEqual(m_ExpectedAppName, appName);
        }

        [Test]
        public void GetAppDisplayName_NoInput_CorrectAppDisplayName()
        {
            // Arrange
            var instance = UnityCloudPlayerSettings.Instance;

            // Act
            var appDisplayName = instance.AppDisplayName;

            // Assert
            Assert.AreEqual(m_ExpectedAppDisplayName, appDisplayName);
        }

        [Test]
        public void GetAppOrganization_NoInput_CorrectAppOrganizationId()
        {
            // Arrange
            var instance = UnityCloudPlayerSettings.Instance;

            // Act
            var appOrganizationId = instance.GetAppOrganization();

            // Assert
            Assert.AreEqual(m_ExpectedAppOrganizationId, appOrganizationId);
        }

        [Test]
        public void GetAppNamespace_NoInput_CorrectAppNamespace()
        {
            // Arrange
            var instance = UnityCloudPlayerSettings.Instance;

            // Act
            var appNamespace = instance.GetAppNamespace();

            // Assert
            Assert.AreEqual(m_ExpectedAppNamespace, appNamespace);
        }
    }
}
