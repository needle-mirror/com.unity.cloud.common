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

        static string GetManifestOnDiskPath(string dataPath)
            => Path.Combine(dataPath, "CertificatePins", "Resources", "CertificatePins", "BuiltPinManifest.txt");

        /// <summary>
        /// Deletes <see cref="BuiltManifestAssetPath"/> (and its <c>.meta</c>) if it exists and logs a Console message.
        /// </summary>
        internal static void DeleteManifestIfExists()
            => DeleteManifestIfExists(Application.dataPath, refreshAssetDatabase: true);

        /// <summary>
        /// Deletes the built manifest under <paramref name="dataPath"/> if it exists and logs a Console message.
        /// Separated from the Unity-dependent overload to allow unit testing without <see cref="Application.dataPath"/>.
        /// </summary>
        /// <param name="dataPath">Root data path (equivalent to <c>Application.dataPath</c>).</param>
        /// <param name="refreshAssetDatabase">When <c>true</c>, uses <see cref="AssetDatabase.DeleteAsset"/> to also remove the <c>.meta</c> file. Pass <c>false</c> in tests.</param>
        internal static void DeleteManifestIfExists(string dataPath, bool refreshAssetDatabase = false)
        {
            var manifestOnDisk = GetManifestOnDiskPath(dataPath);

            if (!File.Exists(manifestOnDisk))
                return;

            if (refreshAssetDatabase)
                AssetDatabase.DeleteAsset(BuiltManifestAssetPath);
            else
                File.Delete(manifestOnDisk);

            Debug.Log($"[CertificatePinning] Certificate pinning is disabled. '{BuiltManifestAssetPath}' has been removed.");
        }

        /// <summary>
        /// Regenerates the built pin manifest from PEM sources on disk.
        /// Skips generation and removes any existing manifest when certificate pinning is disabled.
        /// </summary>
        public static void GenerateManifestFromPemSources()
        {
            var isEnabled = CertificatePinningSettings.IsEnabled();

            GenerateManifestFromPemSources(Application.dataPath, isEnabled, refreshAssetDatabase: true);
        }

        /// <summary>
        /// Core manifest generation logic, decoupled from Unity APIs for testability.
        /// </summary>
        /// <param name="dataPath">Root data path (equivalent to <c>Application.dataPath</c>).</param>
        /// <param name="isEnabled">Whether certificate pinning is currently enabled.</param>
        /// <param name="refreshAssetDatabase">When <c>true</c>, calls <see cref="AssetDatabase.ImportAsset"/> after writing. Pass <c>false</c> in tests.</param>
        internal static void GenerateManifestFromPemSources(string dataPath, bool isEnabled, bool refreshAssetDatabase = false)
        {
            if (!isEnabled)
            {
                DeleteManifestIfExists(dataPath);
                return;
            }

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

            var outputPath = GetManifestOnDiskPath(dataPath);
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            File.WriteAllText(outputPath, json);

            if (refreshAssetDatabase)
                AssetDatabase.ImportAsset(BuiltManifestAssetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
