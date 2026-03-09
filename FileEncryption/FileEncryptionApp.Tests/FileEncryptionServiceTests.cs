using System.Security.Cryptography;
using FileEncryptionApp.Services;
using Xunit;

namespace FileEncryptionApp.Tests;

public class FileEncryptionServiceTests
{
    private readonly FileEncryptionService _servico;
    private readonly string _pastaTeste;

    public FileEncryptionServiceTests()
    {
        _servico = new FileEncryptionService();
        _pastaTeste = Path.Combine(Path.GetTempPath(), "FileEncryptionTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_pastaTeste);
    }

    private void LimparPastaTeste()
    {
        if (Directory.Exists(_pastaTeste))
        {
            try
            {
                Directory.Delete(_pastaTeste, true);
            }
            catch { }
        }
    }

    [Fact]
    public void CriptografarEDescriptografar_ComSenhaCorreta_DeveRestaurarConteudoOriginal()
    {
        try
        {
            var arquivoOriginal = Path.Combine(_pastaTeste, "documento.txt");
            var conteudoOriginal = "Este é o conteúdo secreto do arquivo! 123";
            File.WriteAllText(arquivoOriginal, conteudoOriginal);

            var senha = "MinhaSenhaSegura123";

            var arquivoCriptografado = _servico.CriptografarArquivo(arquivoOriginal, senha);

            Assert.True(File.Exists(arquivoCriptografado));
            var conteudoCriptografado = File.ReadAllBytes(arquivoCriptografado);
            Assert.NotEqual(conteudoOriginal, System.Text.Encoding.UTF8.GetString(conteudoCriptografado));

            var arquivoRestaurado = _servico.DescriptografarArquivo(arquivoCriptografado, senha);

            Assert.True(File.Exists(arquivoRestaurado));
            var conteudoRestaurado = File.ReadAllText(arquivoRestaurado);
            Assert.Equal(conteudoOriginal, conteudoRestaurado);
        }
        finally
        {
            LimparPastaTeste();
        }
    }

    [Fact]
    public void Descriptografar_ComSenhaIncorreta_DeveLancarExcecao()
    {
        try
        {
            var arquivoOriginal = Path.Combine(_pastaTeste, "secreto.txt");
            File.WriteAllText(arquivoOriginal, "Dados confidenciais");

            var arquivoCriptografado = _servico.CriptografarArquivo(arquivoOriginal, "SenhaCorreta");

            Assert.Throws<CryptographicException>(() =>
                _servico.DescriptografarArquivo(arquivoCriptografado, "SenhaErrada"));
        }
        finally
        {
            LimparPastaTeste();
        }
    }

    [Fact]
    public void Criptografar_ArquivoInexistente_DeveLancarExcecaoControlada()
    {
        var arquivoInexistente = Path.Combine(_pastaTeste, "nao_existe.txt");

        var excecao = Assert.Throws<FileNotFoundException>(() =>
            _servico.CriptografarArquivo(arquivoInexistente, "senha"));

        Assert.Contains("não encontrado", excecao.Message, StringComparison.OrdinalIgnoreCase);
        LimparPastaTeste();
    }

    [Fact]
    public void Descriptografar_ArquivoInexistente_DeveLancarExcecaoControlada()
    {
        var arquivoInexistente = Path.Combine(_pastaTeste, "nao_existe.enc");

        var excecao = Assert.Throws<FileNotFoundException>(() =>
            _servico.DescriptografarArquivo(arquivoInexistente, "senha"));

        Assert.Contains("não encontrado", excecao.Message, StringComparison.OrdinalIgnoreCase);
        LimparPastaTeste();
    }
}
