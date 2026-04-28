#nullable enable

using System;
using UnityEditor;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// Regenerates the built pin manifest when PEM sources under <see cref="CertificatePinsManifestGenerator.AssetsCertificatePinsRoot"/> change,
    /// so Editor Play Mode stays in sync without a full build.
    /// </summary>
    sealed class CertificatePinsPemAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ShouldRegenerate(importedAssets) || ShouldRegenerate(deletedAssets) || ShouldRegenerate(movedAssets) ||
                ShouldRegenerate(movedFromAssetPaths))
                CertificatePinsManifestGenerator.GenerateManifestFromPemSources();
        }

        static bool ShouldRegenerate(string[]? paths)
        {
            if (paths == null)
                return false;
            foreach (var p in paths)
            {
                if (p.StartsWith(CertificatePinsManifestGenerator.AssetsCertificatePinsRoot + "/", StringComparison.Ordinal) &&
                    p.IndexOf("/Resources/", StringComparison.Ordinal) < 0 &&
                    p.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
