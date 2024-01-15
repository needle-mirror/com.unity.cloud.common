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
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                PlistElementDict rootDict = plist.root;

                //  remove exit on suspend if it exists
                string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
                if (rootDict.values.ContainsKey(exitsOnSuspendKey))
                {
                    rootDict.values.Remove(exitsOnSuspendKey);
                }

                if (!rootDict.values.ContainsKey("CFBundleURLTypes"))
                {
                    // Create Custom URI Scheme entry
                    var urlTypeArray = new PlistElementArray();
                    var urlDict = urlTypeArray.AddDict();
                    var urlBundleName = new PlistElementString("Unity Cloud App Linking");
                    urlDict.values.Add("CFBundleURLName", urlBundleName);
                    var urlBundleSchemes = new PlistElementArray();
                    urlBundleSchemes.AddString(customUriScheme);
                    urlDict.values.Add("CFBundleURLSchemes", urlBundleSchemes);
                    rootDict.values.Add("CFBundleURLTypes", urlTypeArray);
                }

                File.WriteAllText(plistPath, plist.WriteToString());
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
        }
    }
}
#endif
