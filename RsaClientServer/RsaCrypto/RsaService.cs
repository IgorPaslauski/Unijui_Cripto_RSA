using System.Security.Cryptography;
using System.Text;

namespace RsaCrypto;

public class RsaService
{
    private const int KeySize = 2048;
    private const int PemLineLength = 64;

    /// <summary>Converte chave pública Base64 para formato PEM (para uso em ferramentas externas).</summary>
    public static string ToPublicKeyPem(string publicKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(publicKeyBase64)) return "";
        var b64 = publicKeyBase64.Trim().Replace("\r", "").Replace("\n", "");
        var lines = Chunk(b64, PemLineLength);
        return "-----BEGIN RSA PUBLIC KEY-----\n" + string.Join("\n", lines) + "\n-----END RSA PUBLIC KEY-----";
    }

    /// <summary>Chave privada em PEM PKCS#1 (-----BEGIN RSA PRIVATE KEY-----).</summary>
    public static string ToPrivateKeyPem(string privateKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(privateKeyBase64)) return "";
        var b64 = privateKeyBase64.Trim().Replace("\r", "").Replace("\n", "");
        var lines = Chunk(b64, PemLineLength);
        return "-----BEGIN RSA PRIVATE KEY-----\n" + string.Join("\n", lines) + "\n-----END RSA PRIVATE KEY-----";
    }

    /// <summary>Chave privada em PEM PKCS#8 (-----BEGIN PRIVATE KEY-----) — compatível com mais ferramentas.</summary>
    public static string ToPrivateKeyPkcs8Pem(string privateKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(privateKeyBase64)) return "";
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64.Trim().Replace("\r", "").Replace("\n", "")), out _);
        var pkcs8 = rsa.ExportPkcs8PrivateKey();
        var b64 = Convert.ToBase64String(pkcs8);
        var lines = Chunk(b64, PemLineLength);
        return "-----BEGIN PRIVATE KEY-----\n" + string.Join("\n", lines) + "\n-----END PRIVATE KEY-----";
    }

    static IEnumerable<string> Chunk(string s, int size)
    {
        for (var i = 0; i < s.Length; i += size)
            yield return s.Substring(i, Math.Min(size, s.Length - i));
    }

    public static (string PublicKeyBase64, string PrivateKeyBase64) GenerateKeyPair()
    {
        using var rsa = RSA.Create(KeySize);
        var publicKey = rsa.ExportRSAPublicKey();
        var privateKey = rsa.ExportRSAPrivateKey();
        return (Convert.ToBase64String(publicKey), Convert.ToBase64String(privateKey));
    }

    public static string Encrypt(string plainText, string publicKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("A mensagem não pode ser vazia.", nameof(plainText));

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        if (plainBytes.Length > 245)
            throw new ArgumentException("Mensagem muito longa para RSA 2048. Use no máximo ~240 caracteres.", nameof(plainText));

        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKeyBase64), out _);

        var encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string encryptedBase64, string privateKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(encryptedBase64))
            throw new ArgumentException("A mensagem criptografada não pode ser vazia.", nameof(encryptedBase64));

        var encryptedBytes = Convert.FromBase64String(encryptedBase64);

        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64), out _);

        var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
