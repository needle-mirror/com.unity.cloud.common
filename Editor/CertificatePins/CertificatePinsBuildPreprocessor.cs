#nullable enable

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// Before a player build, refreshes <see cref="CertificatePinsManifestGenerator.BuiltManifestAssetPath"/> from
    /// PEM files under <c>Assets/CertificatePins/</c> (one subfolder per DNS hostname).
    /// </summary>
    sealed class CertificatePinsBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEngine.Debug.Log("OnPreprocessBuild CertificatePinsBuildPreprocessor");
            CertificatePinsManifestGenerator.GenerateManifestFromPemSources();
        }
    }
}
