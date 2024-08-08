#if UNITY_STANDALONE_WIN
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEngine;
using System;

namespace Unity.Cloud.AppLinking.Editor
{
    class WindowsBuildPostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPostprocessBuild(bool isDevelopmentBuild, BuildTarget target, string path)
        {
            if (target == BuildTarget.StandaloneWindows64)
            {
                Debug.Log($"WindowsBuildPostProcess.OnPostprocessBuild");

                var lastFolderIndex = path.LastIndexOf("/", StringComparison.InvariantCulture);

                // UX: Reuse the .exe name of the Unity application being built
                var exeAppName = path.Substring(lastFolderIndex + 1);

                var interopDirectory = $"{path.Substring(0, lastFolderIndex)}/Unity_Cloud_Interop";
                var customUriSchemeResolverDestinationFilePath = $"{interopDirectory}/{exeAppName}";

                if (!Directory.Exists(interopDirectory))
                {
                    Directory.CreateDirectory(interopDirectory);
                }

                // Copy signed executable from ./Tools folder
                var customUriSchemeResolverPath = Path.Combine(Application.dataPath, "../Packages/com.unity.cloud.common/AppLinking/Tools/CustomUriSchemeResolver.exe");
                File.Copy(customUriSchemeResolverPath, customUriSchemeResolverDestinationFilePath, true);
                if (!isDevelopmentBuild) return;
                // If in development mode, copy over .pdb file
                var customUriSchemeResolverDebugSymbolsPath = Path.Combine(Application.dataPath, "../Packages/com.unity.cloud.common/AppLinking/Tools/CustomUriSchemeResolver.pdb");
                var customUriSchemeResolverDebugSymbolsDestinationPath = Path.ChangeExtension(customUriSchemeResolverDestinationFilePath, "pdb");
                File.Copy(customUriSchemeResolverDebugSymbolsPath, customUriSchemeResolverDebugSymbolsDestinationPath, true);
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            OnPostprocessBuild(report.summary.options.HasFlag(BuildOptions.Development), report.summary.platform, report.summary.outputPath);
        }
    }
}
#endif
