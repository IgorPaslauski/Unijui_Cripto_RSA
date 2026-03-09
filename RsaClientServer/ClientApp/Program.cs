using System.Net.Sockets;
using System.Text;
using RsaCrypto;

const int Port = 5000;

Console.WriteLine("=== CLIENTE RSA - Segurança de Dados ===");
Console.WriteLine();

Console.Write("Digite o IP do servidor (Enter para localhost): ");
var host = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(host))
    host = "127.0.0.1";

try
{
    using var client = new TcpClient();
    Console.WriteLine($"[...] Conectando ao servidor em {host}:{Port}...");
    await client.ConnectAsync(host, Port);
    Console.WriteLine("[OK] Conectado ao servidor!");
    Console.WriteLine();

    using var stream = client.GetStream();

    var lengthBuffer = new byte[4];
    var bytesRead = await stream.ReadAsync(lengthBuffer);
    if (bytesRead < 4)
    {
        Console.WriteLine("[ERRO] Não foi possível receber a chave pública.");
        return;
    }

    var keyLength = BitConverter.ToInt32(lengthBuffer, 0);
    if (keyLength <= 0 || keyLength > 10_000)
    {
        Console.WriteLine("[ERRO] Tamanho de chave inválido.");
        return;
    }

    var keyBuffer = new byte[keyLength];
    var totalRead = 0;
    while (totalRead < keyLength)
    {
        var read = await stream.ReadAsync(keyBuffer.AsMemory(totalRead, keyLength - totalRead));
        if (read == 0) break;
        totalRead += read;
    }

    var publicKey = Encoding.UTF8.GetString(keyBuffer, 0, totalRead);
    Console.WriteLine("[OK] Chave pública recebida do servidor.");
    Console.WriteLine();
    Console.WriteLine("Digite suas mensagens (ou 'sair' para encerrar):");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Mensagem: ");
        var message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message) || message.Equals("sair", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Encerrando conexão...");
            break;
        }

        if (message.Length > 240)
        {
            Console.WriteLine("[ERRO] Mensagem muito longa. Use no máximo 240 caracteres.");
            continue;
        }

        var encryptedBase64 = RsaService.Encrypt(message, publicKey);
        var encryptedBytes = Encoding.UTF8.GetBytes(encryptedBase64);
        var lengthBytes = BitConverter.GetBytes(encryptedBytes.Length);
        await stream.WriteAsync(lengthBytes);
        await stream.WriteAsync(encryptedBytes);
        Console.WriteLine("[OK] Mensagem enviada!");
    }

    Console.WriteLine("Pressione qualquer tecla para sair.");
    Console.ReadKey();
}
catch (SocketException ex)
{
    Console.WriteLine($"[ERRO DE CONEXÃO] {ex.Message}");
    Console.WriteLine("Certifique-se de que o servidor está rodando antes de iniciar o cliente.");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERRO] {ex.Message}");
}
