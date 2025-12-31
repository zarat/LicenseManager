using System;
using System.IO;

namespace Zarat.Licensing;

public sealed class LicenseManager
{
    private readonly string _publicKeyPem;
    private readonly string _licensePath;
    private readonly Func<string>? _machineIdProvider;

    private LicenseValidationResult? _cached;

    // Damit funktioniert: new LicenseManager(publicKey, licensePath)
    public LicenseManager(string publicKeyPem, string licensePath)
        : this(publicKeyPem, licensePath, null)
    {
    }

    // Optionaler 3. Parameter
    public LicenseManager(string publicKeyPem, string licensePath, Func<string>? machineIdProvider)
    {
        _publicKeyPem = publicKeyPem ?? throw new ArgumentNullException(nameof(publicKeyPem));
        _licensePath = licensePath ?? throw new ArgumentNullException(nameof(licensePath));
        _machineIdProvider = machineIdProvider;
    }

    public LicenseValidationResult Validate()
    {
        if (_cached is not null) return _cached;

        var lic = LicenseJson.LoadFromFile(_licensePath);
        if (lic is null)
        {
            _cached = new LicenseValidationResult
            {
                Status = LicenseStatus.MissingOrUnreadable,
                Reason = $"Keine Lizenz gefunden oder nicht lesbar: {_licensePath}"
            };
            return _cached;
        }

        _cached = LicenseVerifier.Validate(_publicKeyPem, lic, DateTime.UtcNow, _machineIdProvider);
        return _cached;
    }

    public void EnsureValid()
    {
        var r = Validate();
        if (!r.IsValid)
            throw new InvalidOperationException("Lizenz ungültig: " + r.Reason);
    }
}
