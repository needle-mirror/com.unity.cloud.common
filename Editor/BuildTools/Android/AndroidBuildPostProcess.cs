#if UNITY_ANDROID
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEditor.Android;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// See https://stackoverflow.com/a/54894488 for more info
    /// </summary>
    public class AndroidBuildPostProcess : IPostGenerateGradleAndroidProject
    {
        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            var androidManifest = new AndroidXmlDocument(GetManifestPath(basePath));
            androidManifest.ApplyDeepLinkChanges();
            androidManifest.Save();
        }

        public int callbackOrder { get { return 1; } }

        private string _manifestFilePath;

        private string GetManifestPath(string basePath)
        {
            if (string.IsNullOrEmpty(_manifestFilePath))
            {
                StringBuilder pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                _manifestFilePath = pathBuilder.ToString();
            }
            return _manifestFilePath;
        }
    }

    class AndroidXmlDocument : XmlDocument
    {
        string m_Path;
        protected XmlNamespaceManager nsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (XmlTextReader reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }
            nsMgr = new XmlNamespaceManager(NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);
        }

        public void Save()
        {
            using (XmlTextWriter writer = new XmlTextWriter(m_Path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
        }

        internal XmlElement CreateElementWithAttribute(string elementName, string attributeName, string attributeValue)
        {
            XmlElement element = CreateElement(elementName);
            XmlAttribute attribute = CreateAttribute("android", attributeName, AndroidXmlNamespace);
            attribute.Value = attributeValue;
            element.Attributes.Append(attribute);
            return element;
        }

        internal XmlElement CreateElementWithAttributes(string elementName, Dictionary<string, string> attributes)
        {
            var element = CreateElement(elementName);
            foreach (var attribute in attributes)
            {
                var androidAttribute = CreateAttribute("android", attribute.Key, AndroidXmlNamespace);
                androidAttribute.Value = attribute.Value;
                element.SetAttributeNode(androidAttribute);
            }

            return element;
        }

        internal XmlNode GetActivityWithLaunchIntent()
        {
            return SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                "intent-filter/category/@android:name='android.intent.category.LAUNCHER']", nsMgr);
        }

        internal void ApplyDeepLinkChanges()
        {
            AddDeepLinkIntents(GetActivityWithLaunchIntent());
        }

        internal void AddDeepLinkIntents(XmlNode mainActivity)
        {
            var reflectSchemeIntent = CreateDeepLinkIntent();
            mainActivity.AppendChild(reflectSchemeIntent);
            foreach (var supportedDomain in AppLinksHelper.SupportedDomains)
            {
                var appLinksIntent = CreateAppLinkIntent(supportedDomain);
                mainActivity.AppendChild(appLinksIntent);
            }
        }

        internal XmlElement CreateDeepLinkIntent()
        {
            var customUriScheme = BuildUtils.GetNamespacedUriScheme();
            Debug.Log($"Registering '{customUriScheme}' scheme intent-filter for Android Viewer");
            var intentNode = CreateElement("intent-filter");
            var scheme = CreateElementWithAttribute("data", "scheme", customUriScheme);
            intentNode.AppendChild(scheme);
            AppendIntentAttributes(intentNode);

            return intentNode;
        }

        // TODO from app config, read app link attributes
        // for now, support reflect project rest path
        internal XmlElement CreateAppLinkIntent(string domain)
        {
            var customUriScheme = BuildUtils.GetCustomUriScheme();
            var appLinkAttributes = new Dictionary<string, string>()
            {
                {"scheme", "https"},
                {"host", domain},
                {"pathPrefix", $"/link/{customUriScheme}"}
            };
            var intentNode = CreateElement("intent-filter");
            var scheme = CreateElementWithAttributes("data", appLinkAttributes);
            intentNode.AppendChild(scheme);
            AppendIntentAttributes(intentNode);

            return intentNode;
        }

        internal void AppendIntentAttributes(XmlElement element)
        {
            element.AppendChild(CreateElementWithAttribute("action", "name", "android.intent.action.VIEW"));
            element.AppendChild(CreateElementWithAttribute("category", "name", "android.intent.category.DEFAULT"));
            element.AppendChild(CreateElementWithAttribute("category", "name", "android.intent.category.BROWSABLE"));
        }
    }
}

#endif
