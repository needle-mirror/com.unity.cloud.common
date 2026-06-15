#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// In-memory SPKI (public key) pins loaded once from <see cref="Resources"/> for
    /// <see cref="PublicKeyPinningCertificateValidationPolicy"/>.
    /// </summary>
    sealed class SpkiPublicKeyPinTable
    {
        const string k_ManifestLogTag = "[CertificatePinManifest]";

        readonly Dictionary<string, HostPinSet> m_HostPins;

        SpkiPublicKeyPinTable(Dictionary<string, HostPinSet> hostPins)
        {
            m_HostPins = hostPins;
        }

        internal static SpkiPublicKeyPinTable Load(string resourcesRootPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesRootPath))
                throw new ArgumentException("Resources root path must be non-empty.", nameof(resourcesRootPath));

            var builtPath = $"{resourcesRootPath}/BuiltPinManifest";
            LogManifestDiagnostic(
                $"Load start: path='{builtPath}' platform={Application.platform} dataPath='{Application.dataPath}' streamingAssetsPath='{Application.streamingAssetsPath}'");

            var asset = Resources.Load<TextAsset>(builtPath);
            if (asset == null)
            {
                LogManifestWarning(
                    $"Resources.Load<TextAsset> returned null for '{builtPath}'. No public key pins will be loaded.");
                return new SpkiPublicKeyPinTable(new Dictionary<string, HostPinSet>(StringComparer.OrdinalIgnoreCase));
            }

            var textSource = "TextAsset.text";
            var json = asset.text;
            var textLen = json?.Length ?? 0;
            var bytesLen = asset.bytes?.Length ?? 0;
            LogManifestDiagnostic(
                $"TextAsset loaded: name='{asset.name}' textLength={textLen} bytesLength={bytesLen}");

            if (string.IsNullOrWhiteSpace(json) && bytesLen > 0)
            {
                json = Encoding.UTF8.GetString(asset.bytes);
                textSource = "TextAsset.bytes (UTF-8)";
                LogManifestDiagnostic($"TextAsset.text was empty; decoded JSON from bytes, charLength={json.Length}.");
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                LogManifestWarning(
                    $"Manifest TextAsset has no usable text (textLength={textLen}, bytesLength={bytesLen}). No public key pins will be loaded.");
                return new SpkiPublicKeyPinTable(new Dictionary<string, HostPinSet>(StringComparer.OrdinalIgnoreCase));
            }

            json = json.Trim();
            if (json.Length > 0 && json[0] == '\uFEFF')
                json = json.Substring(1);

            var dto = JsonSerialization.Deserialize<BuiltCertificatePinManifestDto>(json);
            if (dto == null || dto.v != 1 || dto.hosts == null)
            {
                var reason =
                    dto == null
                        ? "JSON deserialize returned null"
                        : dto.hosts == null
                            ? $"unsupported or missing hosts (v={dto.v})"
                            : $"unsupported version (v={dto.v}, expected 1)";
                LogManifestWarning(
                    $"{reason} for '{builtPath}' (contentLength={json.Length}, source={textSource}). Preview: {PreviewForLog(json, 160)}");
                return new SpkiPublicKeyPinTable(new Dictionary<string, HostPinSet>(StringComparer.OrdinalIgnoreCase));
            }

            var hostPins = new Dictionary<string, HostPinSet>(StringComparer.OrdinalIgnoreCase);
            var skippedHosts = 0;
            foreach (var kv in dto.hosts)
            {
                var host = kv.Key?.Trim();
                if (string.IsNullOrEmpty(host) || Uri.CheckHostName(host) != UriHostNameType.Dns)
                {
                    skippedHosts++;
                    LogSkipHost(host ?? string.Empty, "not a DNS hostname");
                    continue;
                }

                var hexSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var hex in kv.Value ?? Enumerable.Empty<string>())
                {
                    if (string.IsNullOrWhiteSpace(hex))
                        continue;
                    hexSet.Add(hex.Trim().ToUpperInvariant());
                }

                if (hexSet.Count > 0)
                    hostPins[host] = HostPinSet.FromSpkiPins(hexSet);
            }

            if (hostPins.Count == 0)
            {
                LogManifestWarning(
                    $"Manifest parsed (v={dto.v}) but no hosts ended up with pins (raw host entries={dto.hosts.Count}, skipped={skippedHosts}).");
            }
            else
            {
                var hostList = string.Join(", ", hostPins.Keys.OrderBy(h => h, StringComparer.OrdinalIgnoreCase));
                LogManifestDiagnostic(
                    $"Loaded OK from '{builtPath}' (source={textSource}): v={dto.v}, pinnedHosts={hostPins.Count} [{hostList}]");
            }

            return new SpkiPublicKeyPinTable(hostPins);
        }

        static string PreviewForLog(string s, int maxChars)
        {
            if (string.IsNullOrEmpty(s))
                return "<empty>";
            var oneLine = s.Replace("\r", " ").Replace("\n", " ").Trim();
            if (oneLine.Length <= maxChars)
                return oneLine;
            return oneLine.Substring(0, maxChars) + "…";
        }

        static void LogManifestDiagnostic(string message)
        {
            Debug.Log($"{k_ManifestLogTag} {message}");
        }

        static void LogManifestWarning(string message)
        {
            Debug.LogWarning($"{k_ManifestLogTag} {message}");
        }

        internal bool TryGetPinsForHost(string host, out HostPinSet pins)
        {
            return m_HostPins.TryGetValue(host, out pins);
        }

        internal static bool IsDnsHttpsHost(Uri? requestUri)
        {
            if (requestUri == null || !string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                return false;

            return Uri.CheckHostName(requestUri.Host) == UriHostNameType.Dns;
        }

        /// <summary>
        /// True when the TLS server certificate DER contains a SubjectPublicKeyInfo whose SHA-256 (hex, uppercase)
        /// matches a loaded pin.
        /// </summary>
        internal static bool MatchesServerCertificateDer(byte[]? serverCertificateDer, HostPinSet pins)
        {
            if (serverCertificateDer == null || pins.IsEmpty)
                return false;

            if (!X509SubjectPublicKeyInfoExtractor.TryExtractFromCertificateDer(serverCertificateDer, out var serverSpki) ||
                serverSpki == null || serverSpki.Length == 0)
            {
#if UNITY_EDITOR || DEBUG
                Debug.LogWarning(
                    $"{k_ManifestLogTag} Could not extract SPKI from server certificate (DER length={serverCertificateDer.Length}).");
#endif
                return false;
            }

            var computedHex = SpkiPublicKeyPinComputation.ComputeSha256Hex(serverSpki);
            var matched = pins.SpkiSha256Hex.Contains(computedHex);
#if UNITY_EDITOR || DEBUG
            if (!matched)
            {
                Debug.LogWarning(
                    $"{k_ManifestLogTag} SPKI pin mismatch: leaf SPKI SHA-256={computedHex} (DER length={serverCertificateDer.Length}, loaded pin count={pins.SpkiSha256Hex.Count}).");
            } 
            else
            {
                Debug.Log($"{k_ManifestLogTag} SPKI pin MATCH: (DER length={serverCertificateDer.Length}, loaded pin count={pins.SpkiSha256Hex.Count}).");
            }
#endif
            return matched;
        }

        internal static bool MatchesServerCertificate(X509Certificate2 certificate, HostPinSet pins)
        {
            return certificate != null && MatchesServerCertificateDer(certificate.RawData, pins);
        }

        static void LogSkipHost(string host, string reason)
        {
            LogManifestWarning($"Skipping manifest host entry '{host}': {reason}.");
        }
    }

    readonly struct HostPinSet
    {
        static readonly HashSet<string> k_EmptySpki = new(StringComparer.OrdinalIgnoreCase);

        public static readonly HostPinSet Empty = new(k_EmptySpki);

        HostPinSet(HashSet<string> spkiSha256Hex)
        {
            SpkiSha256Hex = spkiSha256Hex;
        }

        public HashSet<string> SpkiSha256Hex { get; }

        public bool IsEmpty => SpkiSha256Hex.Count == 0;

        public static HostPinSet FromSpkiPins(HashSet<string> spkiSha256Hex)
        {
            if (spkiSha256Hex.Count == 0)
                return Empty;
            return new HostPinSet(spkiSha256Hex);
        }
    }

    /// <summary>
    /// Extracts the SubjectPublicKeyInfo TLV from TLS server certificate DER (handshake leaf) by walking the
    /// ASN.1 <c>Certificate</c> / <c>TBSCertificate</c> layout in
    /// <see href="https://www.rfc-editor.org/rfc/rfc5280#section-4.1">RFC 5280, Section 4.1</see>, using BER/DER
    /// encoding rules from <see href="https://www.itu.int/rec/T-REC-X.690">ITU-T X.690</see>.
    /// </summary>
    static class X509SubjectPublicKeyInfoExtractor
    {
        internal static bool TryExtractFromCertificateDer(byte[]? certificateDer, out byte[]? spkiTlv)
        {
            spkiTlv = null;
            if (certificateDer == null || certificateDer.Length == 0)
                return false;

            var data = certificateDer.AsSpan();
            var offset = 0;
            if (!DerReader.TryReadElement(data, ref offset, out var certificateSequence) ||
                !DerReader.TryGetContents(certificateSequence, out var certInner))
                return false;

            var c = 0;
            if (!DerReader.TryReadElement(certInner, ref c, out var tbsCertificateSequence))
                return false;

            if (!DerReader.TryGetContents(tbsCertificateSequence, out var tbsInner))
                return false;

            var fields = new List<byte[]>();
            var p = 0;
            while (p < tbsInner.Length)
            {
                if (!DerReader.TryReadElement(tbsInner, ref p, out var fieldTlv))
                    return false;
                fields.Add(fieldTlv.ToArray());
            }

            if (fields.Count < 6)
                return false;

            var start = fields[0].Length > 0 && fields[0][0] == 0xA0 ? 1 : 0;
            if (fields.Count < start + 6)
                return false;

            spkiTlv = fields[start + 5];
            return true;
        }
    }

    /// <summary>
    /// Minimal BER/DER TLV reader (tag, definite length, contents) for constructed <c>SEQUENCE</c> (tag 0x30) per
    /// <see href="https://www.itu.int/rec/T-REC-X.690">ITU-T X.690</see>.
    /// </summary>
    static class DerReader
    {
        internal static bool TryReadElement(ReadOnlySpan<byte> data, ref int offset, out ReadOnlySpan<byte> element)
        {
            element = default;
            var start = offset;
            if (offset >= data.Length)
                return false;

            offset++;
            if (!TryReadLength(data, ref offset, out var contentLength))
                return false;

            if (offset + contentLength > data.Length)
                return false;

            offset += contentLength;
            element = data.Slice(start, offset - start);
            return true;
        }

        internal static bool TryGetContents(ReadOnlySpan<byte> sequenceTlv, out ReadOnlySpan<byte> contents)
        {
            contents = default;
            if (sequenceTlv.Length < 2 || sequenceTlv[0] != 0x30)
                return false;

            var o = 1;
            if (!TryReadLength(sequenceTlv, ref o, out var len))
                return false;

            if (o + len != sequenceTlv.Length)
                return false;

            contents = sequenceTlv.Slice(o, len);
            return true;
        }

        static bool TryReadLength(ReadOnlySpan<byte> data, ref int offset, out int length)
        {
            length = 0;
            if (offset >= data.Length)
                return false;

            var b = data[offset++];
            if (b < 0x80)
            {
                length = b;
                return true;
            }

            var numBytes = b & 0x7F;
            if (numBytes == 0 || numBytes > 4 || offset + numBytes > data.Length)
                return false;

            for (var i = 0; i < numBytes; i++)
                length = (length << 8) | data[offset++];

            return true;
        }
    }
}
