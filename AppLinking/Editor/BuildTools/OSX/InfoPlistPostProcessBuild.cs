#if UNITY_STANDALONE_OSX
using UnityEditor;
using UnityEditor.Build;
//Since the PlistParser doesn't exist in the normal UnityEditor files, we need to add it
using UnityEngine;
using System.IO;
using UnityEditor.Build.Reporting;

namespace Unity.Cloud.AppLinking.Editor
{
    class InfoPlistPostProcessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 1; }
        }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            Debug.Log("InfoPlistPostProcessBuild.OnPostprocessBuild for target " + target + " at path " + path);

            // iOS and OSX share same info.plist entries to support Custom URI Schemes
            if (target == BuildTarget.StandaloneOSX || target == BuildTarget.iOS)
            {
                var customUriScheme = BuildUtils.GetNamespacedUriScheme();
                var plistPath = string.Empty;
                if (target == BuildTarget.StandaloneOSX)
                {
                    if (path.ToLower().EndsWith(".app", System.StringComparison.InvariantCulture))
                    {
                        plistPath = $"{path}/Contents/Info.plist";
                    }
                    else
                    {
                        plistPath = $"{path}/{Application.productName}/Info.plist";
                    }
                }
                if (target == BuildTarget.iOS)
                {
                    plistPath = $"{path}/Info.plist";
                }

                if (File.Exists(plistPath))
                {
                    Debug.Log($"Registering '{customUriScheme}' scheme intent-filter");
                    var plistDocument = new PlistDocument();
                    plistDocument.ReadFromFile(plistPath);
                    var rootDict = plistDocument.root;
                    if (!rootDict.values.ContainsKey("CFBundleURLTypes"))
                    {
                        // Create Custom URI Scheme entry
                        var urlTypeArray = new PlistElementArray();
                        var urlDict = urlTypeArray.AddDict();
                        var urlBundleName = new PlistElementString("Unity Cloud Identity");
                        urlDict.values.Add("CFBundleURLName", urlBundleName);
                        var urlBundleSchemes = new PlistElementArray();
                        urlBundleSchemes.AddString(customUriScheme);
                        urlDict.values.Add("CFBundleURLSchemes", urlBundleSchemes);
                        rootDict.values.Add("CFBundleURLTypes", urlTypeArray);

                        // Write back our changes to Info.plist
                        File.WriteAllText(plistPath, plistDocument.WriteToString());
                    }
                }
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
        }
    }
}
#endif
