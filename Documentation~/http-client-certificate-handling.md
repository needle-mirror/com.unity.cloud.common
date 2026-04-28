# HTTP client certificate handling

Certificate validation is an optional configuration on `UnityHttpClient` and `DotNetHttpClient`, supplied through `ICertificateValidationPolicy`.

## Security guidance

- Do not disable certificate validation in production builds.
- For pinning, use **`PublicKeyPinningCertificateValidationPolicy`** with PEM **`PUBLIC KEY`** files from a **trusted source** (see **Certificate pinning** below).
- Default TLS validation remains enabled unless a policy opts in per request.

## Certificate pinning with `PublicKeyPinningCertificateValidationPolicy`

**`PublicKeyPinningCertificateValidationPolicy`** in `Unity.Cloud.Common.Runtime` provides per domain SSL certificate validation using trusted PEM public key files.

**Trusted source:** Add only PEM public keys that come from a source you trust—for example your PKI or security team, an audited release pipeline.

**Scope:** Pinning applies only to **HTTPS** requests with a DNS host. Hosts without pins in the manifest use normal platform TLS validation.

**Maintenance Cost:** Pins are embedded in the app. Any changes to public keys requires a new build.

### Lay out files under `Assets/CertificatePins/`

1. Create **`Assets/CertificatePins/`** if it does not exist.
2. For each host you want to pin, add a **subfolder whose name is the DNS hostname** (same string as `Uri.Host` after parsing your HTTPS URLs).
3. Put one or more **`.pem`** files in that folder (for example `pubkey.pem`). Multiple keys per host are supported (for example, to support rotation); the handshake succeeds if the server’s SPKI matches **any** loaded pin.

Example:

```text
Assets/CertificatePins/
  service.api.unity.com/
    pubkey.pem
```

### Auto-generated manifest

The Editor regenerates automatically:

```text
Assets/CertificatePins/Resources/CertificatePins/BuiltPinManifest.txt
```

whenever you **build a player** (preprocess build step) or **enter Play Mode** in the Editor.

At runtime the policy is loaded at startup to provide the list of hosts covered by Public Key pinning validation. If the asset is missing all hosts use default platform TLS validation.

### Use the policy on your HTTP client

```csharp
using Unity.Cloud.Common.Runtime;

var policy = new PublicKeyPinningCertificateValidationPolicy();
var httpClient = new UnityHttpClient(policy);
```

### Pinning behavior

- **Pin value:** SHA-256 of the SPKI DER bytes, hex **uppercase**, compared to the server leaf’s SPKI during TLS validation.
- **Custom validation** runs only for hosts that have at least one loaded pin; **chain and policy checks** follow the HTTP client (for example `SslPolicyErrors.None` on the .NET path).

