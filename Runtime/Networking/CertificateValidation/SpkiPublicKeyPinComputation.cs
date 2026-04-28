#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// Shared SPKI (public key) pin hashing for Editor build steps and runtime Resources loading.
    /// </summary>
    static class SpkiPublicKeyPinComputation
    {
        internal static void CollectPinsFromPublicKeyMaterial(
            string? text,
            byte[]? binary,
            HashSet<string> spkiPins,
            Action<string>? logCertificatePemNotSupported = null)
        {
            if (!string.IsNullOrEmpty(text) && text.IndexOf("-----BEGIN", StringComparison.Ordinal) >= 0)
            {
                if (text.IndexOf("BEGIN CERTIFICATE", StringComparison.Ordinal) >= 0)
                    logCertificatePemNotSupported?.Invoke(
                        "PEM X.509 certificates are not supported; use PEM 'PUBLIC KEY' (SubjectPublicKeyInfo) or raw SPKI DER instead.");

                foreach (var der in PemDecoder.DecodeAllSections(text, "PUBLIC KEY"))
                    AddSpkiPin(der, spkiPins);

                return;
            }

            if (binary == null || binary.Length < 32)
                return;

            AddSpkiPin(binary, spkiPins);
        }

        internal static void AddSpkiPin(byte[] spkiDer, HashSet<string> spkiPins)
        {
            if (spkiDer == null || spkiDer.Length == 0)
                return;
            spkiPins.Add(ComputeSha256Hex(spkiDer));
        }

        internal static string ComputeSha256Hex(byte[] data)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(data);
            return ToHexUpper(hash);
        }

        static string ToHexUpper(byte[] bytes)
        {
            var c = new char[bytes.Length * 2];
            var i = 0;
            foreach (var b in bytes)
            {
                c[i++] = ToHexChar(b >> 4);
                c[i++] = ToHexChar(b & 0xF);
            }

            return new string(c);
        }

        static char ToHexChar(int v) => (char)(v < 10 ? '0' + v : 'A' + (v - 10));

        static class PemDecoder
        {
            internal static IEnumerable<byte[]> DecodeAllSections(string pem, string label)
            {
                var begin = $"-----BEGIN {label}-----";
                var end = $"-----END {label}-----";
                var idx = 0;
                while (idx < pem.Length)
                {
                    var start = pem.IndexOf(begin, idx, StringComparison.Ordinal);
                    if (start < 0)
                        yield break;

                    start += begin.Length;
                    var finish = pem.IndexOf(end, start, StringComparison.Ordinal);
                    if (finish < 0)
                        yield break;

                    var base64 = pem.Substring(start, finish - start)
                        .Replace("\r", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace(" ", string.Empty);

                    byte[] der;
                    try
                    {
                        der = Convert.FromBase64String(base64);
                    }
                    catch (FormatException)
                    {
                        idx = finish + end.Length;
                        continue;
                    }

                    yield return der;
                    idx = finish + end.Length;
                }
            }
        }
    }
}
