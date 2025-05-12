#if UNITY_IOS

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Unity.Cloud.AppLinking.Editor
{
    class XCodePostProcessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS)
            {
                Debug.Log("XCodePostProcessBuild.OnPostprocessBuild for target " + target + " at path " + path);

                var customUriScheme = BuildUtils.GetNamespacedUriScheme();

                //  edit project file
                var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
                PBXProject pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);
                string frameworkGuid = pbxProject.GetUnityFrameworkTargetGuid();

                //  include Safari Framework to support captive Safari Browser usage
                pbxProject.AddFrameworkToProject(frameworkGuid, "SafariServices.framework", false);

                pbxProject.WriteToFile(projectPath);

                //  edit plist file
                string plistPath = path + "/Info.plist";
                var plistDocument = new PlistDocument();
                plistDocument.ReadFromFile(plistPath);
                var rootDict = plistDocument.root;

                //  remove exit on suspend if it exists
                string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
                if (rootDict.values.ContainsKey(exitsOnSuspendKey))
                {
                    rootDict.values.Remove(exitsOnSuspendKey);
                }

                Debug.Log($"Registering '{customUriScheme}' scheme intent-filter");

                var urlTypeArray = rootDict.values.ContainsKey("CFBundleURLTypes") ? rootDict.values["CFBundleURLTypes"].AsArray() : new PlistElementArray();
                var urlDict = urlTypeArray.AddDict();
                var urlBundleName = new PlistElementString("Unity Cloud App linking");
                urlDict.values.Add("CFBundleURLName", urlBundleName);
                var urlBundleSchemes = new PlistElementArray();
                urlBundleSchemes.AddString(customUriScheme);
                urlDict.values.Add("CFBundleURLSchemes", urlBundleSchemes);

                // Only add if none exists
                rootDict.values.TryAdd("CFBundleURLTypes", urlTypeArray);

                // Write back our changes to Info.plist
                File.WriteAllText(plistPath, plistDocument.WriteToString());
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
        }
    }
}
#endif
