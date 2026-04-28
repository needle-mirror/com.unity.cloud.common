#nullable enable

using System.Collections.Generic;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// JSON shape for <c>CertificatePins/BuiltPinManifest</c>, produced by the Editor from trusted PEM <c>PUBLIC KEY</c> files under
    /// <c>Assets/CertificatePins/</c> (per-host subfolders), and deserialized at runtime via <see cref="Unity.Cloud.Common.JsonSerialization"/>.
    /// </summary>
    internal sealed class BuiltCertificatePinManifestDto
    {
        public int v { get; set; }

        public Dictionary<string, List<string>>? hosts { get; set; }
    }
}
