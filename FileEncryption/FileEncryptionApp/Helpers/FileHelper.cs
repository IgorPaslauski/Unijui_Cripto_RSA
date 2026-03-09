namespace FileEncryptionApp.Helpers;

public static class FileHelper
{
    public static bool ArquivoExiste(string caminhoArquivo)
    {
        return File.Exists(caminhoArquivo);
    }

    public static byte[] LerArquivo(string caminhoArquivo)
    {
        if (!ArquivoExiste(caminhoArquivo))
        {
            throw new FileNotFoundException("Arquivo não encontrado.", caminhoArquivo);
        }

        return File.ReadAllBytes(caminhoArquivo);
    }

    public static void EscreverArquivo(string caminhoArquivo, byte[] dados)
    {
        File.WriteAllBytes(caminhoArquivo, dados);
    }

    public static string ObterCaminhoArquivoCriptografado(string caminhoOriginal)
    {
        return caminhoOriginal + ".enc";
    }

    public static string ObterCaminhoArquivoRestaurado(string caminhoArquivoCriptografado)
    {
        string caminhoSemEnc = caminhoArquivoCriptografado.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)
            ? caminhoArquivoCriptografado[..^4]
            : caminhoArquivoCriptografado;

        string diretorio = Path.GetDirectoryName(caminhoSemEnc) ?? ".";
        string nomeBase = Path.GetFileNameWithoutExtension(caminhoSemEnc);
        string extensao = Path.GetExtension(caminhoSemEnc);

        return Path.Combine(diretorio, $"{nomeBase}_restaurado{extensao}");
    }
}
