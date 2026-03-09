using RsaCrypto;

namespace RsaCrypto.Tests;

public class RsaServiceTests
{
    [Fact]
    public void GenerateKeyPair_DeveRetornarChavesNaoVazias()
    {
        var (publicKey, privateKey) = RsaService.GenerateKeyPair();

        Assert.False(string.IsNullOrEmpty(publicKey));
        Assert.False(string.IsNullOrEmpty(privateKey));
        Assert.NotEqual(publicKey, privateKey);
    }

    [Fact]
    public void Encrypt_Decrypt_DeveRecuperarMensagemOriginal()
    {
        var (publicKey, privateKey) = RsaService.GenerateKeyPair();
        var mensagemOriginal = "Olá, Segurança de Dados!";

        var criptografada = RsaService.Encrypt(mensagemOriginal, publicKey);
        var descriptografada = RsaService.Decrypt(criptografada, privateKey);

        Assert.NotEqual(mensagemOriginal, criptografada);
        Assert.Equal(mensagemOriginal, descriptografada);
    }

    [Fact]
    public void Encrypt_ComChavePublica_DeveProduzirTextoDiferente()
    {
        var (publicKey, _) = RsaService.GenerateKeyPair();
        var mensagem = "Teste";

        var criptografada = RsaService.Encrypt(mensagem, publicKey);

        Assert.NotEqual(mensagem, criptografada);
        Assert.True(Convert.FromBase64String(criptografada).Length > 0);
    }

    [Fact]
    public void Encrypt_MensagemVazia_DeveLancarExcecao()
    {
        var (publicKey, _) = RsaService.GenerateKeyPair();

        Assert.Throws<ArgumentException>(() => RsaService.Encrypt("", publicKey));
        Assert.Throws<ArgumentException>(() => RsaService.Encrypt("   ", publicKey));
    }

    [Fact]
    public void Decrypt_MensagemVazia_DeveLancarExcecao()
    {
        var (_, privateKey) = RsaService.GenerateKeyPair();

        Assert.Throws<ArgumentException>(() => RsaService.Decrypt("", privateKey));
    }

    [Fact]
    public void Encrypt_Decrypt_ComCaracteresEspeciais_DeveFuncionar()
    {
        var (publicKey, privateKey) = RsaService.GenerateKeyPair();
        var mensagem = "Ação! @#$% 123 çãõ";

        var criptografada = RsaService.Encrypt(mensagem, publicKey);
        var descriptografada = RsaService.Decrypt(criptografada, privateKey);

        Assert.Equal(mensagem, descriptografada);
    }
}
