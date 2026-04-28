#nullable enable

using UnityEditor;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// Regenerates <see cref="CertificatePinsManifestGenerator.BuiltManifestAssetPath"/> when entering Play Mode in the Editor,
    /// matching player builds where <see cref="CertificatePinsBuildPreprocessor"/> runs. PEM import changes are still handled by
    /// <see cref="CertificatePinsPemAssetPostprocessor"/>.
    /// </summary>
    [InitializeOnLoad]
    static class CertificatePinsPlayModeHook
    {
        static CertificatePinsPlayModeHook()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) {
                UnityEngine.Debug.Log("OnPlayModeStateChanged CertificatePinsPlayModeHook");
                CertificatePinsManifestGenerator.GenerateManifestFromPemSources();
            }
        }
    }
}
