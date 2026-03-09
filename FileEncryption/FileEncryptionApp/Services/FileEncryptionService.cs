using System.Security.Cryptography;
using FileEncryptionApp.Helpers;

namespace FileEncryptionApp.Services;

public class FileEncryptionService
{
    private const int TamanhoChaveBytes = 32;
    private const int TamanhoIvBytes = 16;
    private const int TamanhoSaltBytes = 32;
    private const int IteracoesPbkdf2 = 100_000;

    public string CriptografarArquivo(string caminhoArquivoOriginal, string senha)
    {
        if (!FileHelper.ArquivoExiste(caminhoArquivoOriginal))
        {
            throw new FileNotFoundException("Arquivo não encontrado.", caminhoArquivoOriginal);
        }

        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));
        }

        byte[] conteudoOriginal = FileHelper.LerArquivo(caminhoArquivoOriginal);

        byte[] salt = new byte[TamanhoSaltBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] chave = DerivarChave(senha, salt);

        byte[] iv = new byte[TamanhoIvBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        byte[] conteudoCifrado;
        using (var aes = Aes.Create())
        {
            aes.Key = chave;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor();
            conteudoCifrado = encryptor.TransformFinalBlock(conteudoOriginal, 0, conteudoOriginal.Length);
        }

        byte[] arquivoCompleto = new byte[salt.Length + iv.Length + conteudoCifrado.Length];
        Buffer.BlockCopy(salt, 0, arquivoCompleto, 0, salt.Length);
        Buffer.BlockCopy(iv, 0, arquivoCompleto, salt.Length, iv.Length);
        Buffer.BlockCopy(conteudoCifrado, 0, arquivoCompleto, salt.Length + iv.Length, conteudoCifrado.Length);

        string caminhoSaida = FileHelper.ObterCaminhoArquivoCriptografado(caminhoArquivoOriginal);
        FileHelper.EscreverArquivo(caminhoSaida, arquivoCompleto);

        return caminhoSaida;
    }

    public string DescriptografarArquivo(string caminhoArquivoCriptografado, string senha)
    {
        if (!FileHelper.ArquivoExiste(caminhoArquivoCriptografado))
        {
            throw new FileNotFoundException("Arquivo não encontrado.", caminhoArquivoCriptografado);
        }

        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));
        }

        byte[] dadosArquivo = FileHelper.LerArquivo(caminhoArquivoCriptografado);

        int tamanhoMinimo = TamanhoSaltBytes + TamanhoIvBytes;
        if (dadosArquivo.Length < tamanhoMinimo)
        {
            throw new CryptographicException("Arquivo corrompido ou inválido: tamanho insuficiente.");
        }
        byte[] salt = new byte[TamanhoSaltBytes];
        byte[] iv = new byte[TamanhoIvBytes];
        int tamanhoCifrado = dadosArquivo.Length - tamanhoMinimo;
        byte[] conteudoCifrado = new byte[tamanhoCifrado];

        Buffer.BlockCopy(dadosArquivo, 0, salt, 0, TamanhoSaltBytes);
        Buffer.BlockCopy(dadosArquivo, TamanhoSaltBytes, iv, 0, TamanhoIvBytes);
        Buffer.BlockCopy(dadosArquivo, tamanhoMinimo, conteudoCifrado, 0, tamanhoCifrado);

        byte[] chave = DerivarChave(senha, salt);
        byte[] conteudoOriginal;
        try
        {
            using var aes = Aes.Create();
            aes.Key = chave;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using var decryptor = aes.CreateDecryptor();
            conteudoOriginal = decryptor.TransformFinalBlock(conteudoCifrado, 0, conteudoCifrado.Length);
        }
        catch (CryptographicException)
        {
            throw new CryptographicException("Senha incorreta ou arquivo corrompido. Verifique a senha e tente novamente.");
        }
        string caminhoSaida = FileHelper.ObterCaminhoArquivoRestaurado(caminhoArquivoCriptografado);
        FileHelper.EscreverArquivo(caminhoSaida, conteudoOriginal);

        return caminhoSaida;
    }

    private static byte[] DerivarChave(string senha, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            senha,
            salt,
            IteracoesPbkdf2,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(TamanhoChaveBytes);
    }
}
