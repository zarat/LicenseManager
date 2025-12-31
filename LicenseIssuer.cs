using System;
using System.Security.Cryptography;

namespace Zarat.Licensing;

public static class LicenseIssuer
{
    public static SignedLicense CreateSignedLicense(string privateKeyPem, LicensePayload payload)
    {
        if (string.IsNullOrWhiteSpace(privateKeyPem))
            throw new ArgumentException("PrivateKey PEM fehlt.", nameof(privateKeyPem));

        var normalized = LicenseSigning.Normalize(payload);
        var data = LicenseSigning.GetSigningBytes(normalized);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var sig = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return new SignedLicense
        {
            Payload = normalized,
            Signature = Convert.ToBase64String(sig)
        };
    }
}
