using System;
using System.Security.Cryptography;
using System.Text;

namespace Zarat.Licensing;

public static class LicenseVerifier
{
    public static LicenseValidationResult Validate(
        string publicKeyPem,
        SignedLicense license,
        DateTime nowUtc,
        Func<string>? getMachineId = null)
    {
        if (string.IsNullOrWhiteSpace(publicKeyPem))
        {
            return new LicenseValidationResult
            {
                Status = LicenseStatus.InvalidData,
                Reason = "PublicKey PEM fehlt."
            };
        }

        if (license?.Payload is null || string.IsNullOrWhiteSpace(license.Signature))
        {
            return new LicenseValidationResult
            {
                Status = LicenseStatus.InvalidData,
                Reason = "Lizenz unvollständig."
            };
        }

        var payload = LicenseSigning.Normalize(license.Payload);

        // 1) Expiry
        var utcNow = nowUtc.Kind == DateTimeKind.Utc ? nowUtc : nowUtc.ToUniversalTime();
        if (utcNow > payload.ExpiresUtc)
        {
            return new LicenseValidationResult
            {
                Status = LicenseStatus.Expired,
                Reason = "Lizenz ist abgelaufen.",
                Payload = payload
            };
        }

        // 2) Machine binding (optional)
        if (!string.IsNullOrWhiteSpace(payload.MachineId))
        {
            if (getMachineId is null)
            {
                return new LicenseValidationResult
                {
                    Status = LicenseStatus.MachineMismatch,
                    Reason = "Lizenz ist an einen Rechner gebunden, aber es wurde keine MachineId-Funktion geliefert.",
                    Payload = payload
                };
            }

            var local = getMachineId()?.Trim() ?? string.Empty;
            if (!FixedTimeEquals(payload.MachineId, local))
            {
                return new LicenseValidationResult
                {
                    Status = LicenseStatus.MachineMismatch,
                    Reason = "Lizenz ist nicht für diesen Rechner ausgestellt.",
                    Payload = payload
                };
            }
        }

        // 3) Signature verify
        byte[] sig;
        try
        {
            sig = Convert.FromBase64String(license.Signature);
        }
        catch
        {
            return new LicenseValidationResult
            {
                Status = LicenseStatus.InvalidData,
                Reason = "Signatur ist kein gültiges Base64 String.",
                Payload = payload
            };
        }

        var data = LicenseSigning.GetSigningBytes(payload);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        var ok = rsa.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return ok
            ? new LicenseValidationResult
            {
                Status = LicenseStatus.Valid,
                Reason = "OK",
                Payload = payload
            }
            : new LicenseValidationResult
            {
                Status = LicenseStatus.InvalidSignature,
                Reason = "Signatur ungültig (Lizenz manipuliert oder nicht offiziell signiert).",
                Payload = payload
            };
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a ?? string.Empty);
        var bb = Encoding.UTF8.GetBytes(b ?? string.Empty);
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
