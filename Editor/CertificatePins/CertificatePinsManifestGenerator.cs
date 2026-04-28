#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Common.Editor
{
    /// <summary>
    /// Generates <c>Assets/CertificatePins/Resources/CertificatePins/BuiltPinManifest.txt</c> from
    /// <c>Assets/CertificatePins/</c> (one subfolder per DNS hostname, each with <c>*.pem</c> public keys) using the same SPKI hashing as
    /// <see cref="PublicKeyPinningCertificateValidationPolicy"/>. PEM files must come from a <c>trusted source</c>; the generated manifest is only as trustworthy as those inputs.
    /// Regeneration runs before player builds, when PEM sources are imported, and when entering Editor Play Mode.
    /// </summary>
    static class CertificatePinsManifestGenerator
    {
        internal const string AssetsCertificatePinsRoot = "Assets/CertificatePins";
        internal const string BuiltManifestAssetPath = "Assets/CertificatePins/Resources/CertificatePins/BuiltPinManifest.txt";

        /// <summary>
        /// Regenerates the built pin manifest from PEM sources on disk.
        /// </summary>
        public static void GenerateManifestFromPemSources()
        {
            var dataPath = Application.dataPath;
            var rootOnDisk = Path.Combine(dataPath, "CertificatePins");
            var hosts = new Dictionary<string, List<string>>();

            if (Directory.Exists(rootOnDisk))
            {
                foreach (var hostDir in Directory.EnumerateDirectories(rootOnDisk))
                {
                    var host = Path.GetFileName(hostDir);
                    if (string.Equals(host, "Resources", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (Uri.CheckHostName(host) != UriHostNameType.Dns)
                    {
                        Debug.LogWarning(
                            $"[CertificatePins] Skipping folder '{host}': not a DNS hostname (expected FQDN matching HTTPS Uri.Host).");
                        continue;
                    }

                    var pins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var pemPath in Directory.EnumerateFiles(hostDir, "*.pem", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var text = File.ReadAllText(pemPath);
                            SpkiPublicKeyPinComputation.CollectPinsFromPublicKeyMaterial(
                                text,
                                binary: null,
                                pins,
                                msg => Debug.LogWarning($"[CertificatePins] Host '{host}', file '{pemPath}': {msg}"));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[CertificatePins] Host '{host}', file '{pemPath}': {ex.Message}");
                        }
                    }

                    if (pins.Count > 0)
                        hosts[host] = pins.ToList();
                }
            }

            var dto = new BuiltCertificatePinManifestDto { v = 1, hosts = hosts };
            var json = JsonSerialization.Serialize(dto);

            var outputPath = Path.Combine(dataPath, "CertificatePins", "Resources", "CertificatePins", "BuiltPinManifest.txt");
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            File.WriteAllText(outputPath, json);
            AssetDatabase.ImportAsset(BuiltManifestAssetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
