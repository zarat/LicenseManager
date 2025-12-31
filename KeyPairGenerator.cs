using System.IO;
using System.Security.Cryptography;

namespace Zarat.Licensing;

public static class KeyPairGenerator
{
    /// <summary>
    /// Erzeugt RSA Keypair (PEM). Private Key niemals ausliefern.
    /// </summary>
    public static void GenerateRsaPemFiles(string privateKeyPemPath, string publicKeyPemPath, int keySize = 3072)
    {
        using var rsa = RSA.Create(keySize);

        var privatePem = rsa.ExportPkcs8PrivateKeyPem();
        var publicPem = rsa.ExportSubjectPublicKeyInfoPem();

        File.WriteAllText(privateKeyPemPath, privatePem);
        File.WriteAllText(publicKeyPemPath, publicPem);
    }
}
