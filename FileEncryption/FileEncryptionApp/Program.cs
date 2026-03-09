using FileEncryptionApp.Services;
using System.Security.Cryptography;

namespace FileEncryptionApp;

internal static class Program
{
    private static readonly FileEncryptionService ServicoCriptografia = new();

    private static void Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Criptografia de Arquivos - AES");
        Console.WriteLine("  Atividade: Segurança de Dados");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        bool continuar = true;
        while (continuar)
        {
            ExibirMenu();
            string? opcao = Console.ReadLine()?.Trim();

            switch (opcao)
            {
                case "1":
                    ExecutarCriptografia();
                    break;
                case "2":
                    ExecutarDescriptografia();
                    break;
                case "3":
                    Console.WriteLine("Encerrando aplicação. Até logo!");
                    continuar = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida. Digite 1, 2 ou 3.");
                    break;
            }

            if (continuar)
            {
                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey(true);
                Console.Clear();
            }
        }
    }

    private static void ExibirMenu()
    {
        Console.WriteLine("MENU PRINCIPAL");
        Console.WriteLine("--------------");
        Console.WriteLine("1) Criptografar arquivo");
        Console.WriteLine("2) Descriptografar arquivo");
        Console.WriteLine("3) Sair");
        Console.WriteLine();
        Console.Write("Escolha uma opção: ");
    }

    private static void ExecutarCriptografia()
    {
        Console.WriteLine();
        Console.WriteLine("--- CRIPTOGRAFAR ARQUIVO ---");

        Console.Write("Informe o caminho do arquivo original: ");
        string? caminhoOriginal = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(caminhoOriginal))
        {
            Console.WriteLine("[ERRO] Caminho do arquivo não pode ser vazio.");
            return;
        }

        Console.Write("Informe a senha: ");
        string? senha = LerSenha();

        if (string.IsNullOrWhiteSpace(senha))
        {
            Console.WriteLine("[ERRO] A senha não pode ser vazia.");
            return;
        }

        try
        {
            string caminhoCriptografado = ServicoCriptografia.CriptografarArquivo(caminhoOriginal, senha);
            Console.WriteLine($"[OK] Arquivo criptografado com sucesso!");
            Console.WriteLine($"     Salvo em: {caminhoCriptografado}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[ERRO] Arquivo não encontrado: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[ERRO] {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ERRO] Erro de leitura/escrita: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ERRO] Sem permissão para acessar o arquivo: {ex.Message}");
        }
    }

    private static void ExecutarDescriptografia()
    {
        Console.WriteLine();
        Console.WriteLine("--- DESCRIPTOGRAFAR ARQUIVO ---");

        Console.Write("Informe o caminho do arquivo .enc: ");
        string? caminhoEnc = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(caminhoEnc))
        {
            Console.WriteLine("[ERRO] Caminho do arquivo não pode ser vazio.");
            return;
        }

        Console.Write("Informe a senha: ");
        string? senha = LerSenha();

        if (string.IsNullOrWhiteSpace(senha))
        {
            Console.WriteLine("[ERRO] A senha não pode ser vazia.");
            return;
        }

        try
        {
            string caminhoRestaurado = ServicoCriptografia.DescriptografarArquivo(caminhoEnc, senha);
            Console.WriteLine($"[OK] Arquivo descriptografado com sucesso!");
            Console.WriteLine($"     Restaurado em: {caminhoRestaurado}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[ERRO] Arquivo não encontrado: {ex.Message}");
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine($"[ERRO] {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[ERRO] {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ERRO] Erro de leitura/escrita: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ERRO] Sem permissão para acessar o arquivo: {ex.Message}");
        }
    }

    private static string? LerSenha()
    {
        var senha = new System.Text.StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Backspace && senha.Length > 0)
            {
                senha.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                senha.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return senha.ToString();
    }
}
