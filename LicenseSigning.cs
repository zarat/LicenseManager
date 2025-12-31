using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zarat.Licensing;

public static class LicenseSigning
{
    /// <summary>
    /// Normalisiert die Payload deterministisch (UTC, Trim, Features sortiert/deduped).
    /// Diese Regeln müssen für Issuer und Client identisch sein.
    /// </summary>
    public static LicensePayload Normalize(LicensePayload p)
    {
        if (p is null) throw new ArgumentNullException(nameof(p));

        var customer = (p.Customer ?? string.Empty).Trim();

        var expiresUtc = p.ExpiresUtc.Kind switch
        {
            DateTimeKind.Utc => p.ExpiresUtc,
            DateTimeKind.Local => p.ExpiresUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(p.ExpiresUtc, DateTimeKind.Utc) // best effort
        };

        var machineId = string.IsNullOrWhiteSpace(p.MachineId) ? null : p.MachineId.Trim();

        var features = (p.Features ?? Array.Empty<string>())
            .Select(f => (f ?? string.Empty).Trim())
            .Where(f => f.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        return new LicensePayload
        {
            Version = p.Version <= 0 ? 1 : p.Version,
            Customer = customer,
            ExpiresUtc = expiresUtc,
            Features = features,
            MachineId = machineId
        };
    }

    /// <summary>
    /// Liefert die Bytes, die signiert/verifiziert werden.
    /// Keine JSON-Abhängigkeit, deterministisch.
    /// </summary>
    public static byte[] GetSigningBytes(LicensePayload payload)
    {
        var p = Normalize(payload);

        // Wichtig: exakt dieses Format muss auf beiden Seiten gleich bleiben.
        var text =
            $"v={p.Version}\n" +
            $"customer={p.Customer}\n" +
            $"expiresUtc={p.ExpiresUtc:O}\n" +
            $"features={string.Join(",", p.Features)}\n" +
            $"machineId={(p.MachineId ?? string.Empty)}";

        return Encoding.UTF8.GetBytes(text);
    }

    public static bool HasFeature(LicensePayload payload, string feature)
    {
        if (payload?.Features is null || string.IsNullOrWhiteSpace(feature)) return false;
        return payload.Features.Contains(feature.Trim(), StringComparer.Ordinal);
    }
}
