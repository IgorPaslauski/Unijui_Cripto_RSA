using System.Security.Cryptography;
using System.Text;

namespace RsaCrypto;

public class RsaService
{
    private const int KeySize = 2048;

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
