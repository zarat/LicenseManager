using System;
using System.Collections.Generic;

namespace Zarat.Licensing;

public sealed record LicensePayload
{
    // Erhöhen, falls du das Signaturformat jemals änderst.
    public int Version { get; init; } = 1;

    public string Customer { get; init; } = string.Empty;

    // Muss UTC sein (wir normalisieren beim Signieren/Prüfen).
    public DateTime ExpiresUtc { get; init; }

    public IReadOnlyList<string> Features { get; init; } = Array.Empty<string>();

    // Optional: Rechnerbindung. Wenn gesetzt, muss Client exakt diesen Wert liefern.
    public string? MachineId { get; init; }
}

public sealed record SignedLicense
{
    public LicensePayload Payload { get; init; } = new();
    public string Signature { get; init; } = string.Empty; // Base64
}

public enum LicenseStatus
{
    Valid,
    MissingOrUnreadable,
    Expired,
    MachineMismatch,
    InvalidSignature,
    InvalidData
}

public sealed record LicenseValidationResult
{
    public LicenseStatus Status { get; init; }
    public string Reason { get; init; } = string.Empty;
    public LicensePayload? Payload { get; init; }

    public bool IsValid => Status == LicenseStatus.Valid;
}
