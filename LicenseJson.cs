using System;
using System.IO;
using System.Text.Json;

namespace Zarat.Licensing;

public static class LicenseJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static string Serialize(SignedLicense license)
        => JsonSerializer.Serialize(license, Options);

    public static SignedLicense? Deserialize(string json)
        => JsonSerializer.Deserialize<SignedLicense>(json);

    public static SignedLicense? LoadFromFile(string path)
    {
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        return Deserialize(json);
    }

    public static void SaveToFile(string path, SignedLicense license)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, Serialize(license));
    }
}
