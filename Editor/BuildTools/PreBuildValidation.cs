using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    class PreBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 1; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var customUriScheme = BuildUtils.GetCustomUriScheme();
            Debug.Log($"Built Unity App will use '{customUriScheme}' custom uri scheme.");
        }
    }
}
