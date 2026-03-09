using System.Net;
using System.Net.Sockets;
using System.Text;
using RsaCrypto;

const int Port = 5000;
var endereco = args.Length > 0 ? IPAddress.Parse(args[0]) : IPAddress.Any;
var clientId = 0;

Console.WriteLine("=== SERVIDOR RSA - Segurança de Dados ===");
Console.WriteLine();

var (publicKey, privateKey) = RsaService.GenerateKeyPair();
Console.WriteLine("[OK] Par de chaves RSA gerado (2048 bits)");
Console.WriteLine();

var listener = new TcpListener(endereco, Port);
listener.Start();
Console.WriteLine($"[OK] Servidor escutando em {endereco}:{Port}");
Console.WriteLine("[...] Aguardando conexões (Ctrl+C para encerrar)...");
Console.WriteLine();

try
{
    while (true)
    {
        var client = await listener.AcceptTcpClientAsync();
        var id = Interlocked.Increment(ref clientId);
        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "?";
        _ = Task.Run(async () => await TratarCliente(client, id, endpoint, publicKey, privateKey));
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[ERRO] {ex.Message}");
}
finally
{
    listener.Stop();
    Console.WriteLine();
    Console.WriteLine("Servidor encerrado. Pressione qualquer tecla para sair.");
    Console.ReadKey();
}

static async Task TratarCliente(TcpClient client, int id, string endpoint, string publicKey, string privateKey)
{
    try
    {
        using (client)
        using (var stream = client.GetStream())
        {
            Console.WriteLine($"[Cliente {id}] Conectado de {endpoint}");

            var publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
            var lengthBytes = BitConverter.GetBytes(publicKeyBytes.Length);
            await stream.WriteAsync(lengthBytes);
            await stream.WriteAsync(publicKeyBytes);

            var chaveExibicao = publicKey.Length > 80 ? publicKey[..80] + "..." : publicKey;
            Console.WriteLine($"[Cliente {id}] Chave pública enviada: {chaveExibicao}");
            Console.WriteLine();

            while (true)
            {
                var lengthBuffer = new byte[4];
                var bytesRead = await stream.ReadAsync(lengthBuffer);
                if (bytesRead == 0)
                    break;

                if (bytesRead < 4)
                    break;

                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                if (messageLength <= 0 || messageLength > 100_000)
                    break;

                var encryptedBuffer = new byte[messageLength];
                var totalRead = 0;
                while (totalRead < messageLength)
                {
                    var read = await stream.ReadAsync(encryptedBuffer.AsMemory(totalRead, messageLength - totalRead));
                    if (read == 0) break;
                    totalRead += read;
                }

                if (totalRead < messageLength)
                    break;

                var encryptedBase64 = Encoding.UTF8.GetString(encryptedBuffer, 0, totalRead);
                var decryptedMessage = RsaService.Decrypt(encryptedBase64, privateKey);

                Console.WriteLine($"[Cliente {id}] Mensagem: {decryptedMessage}");
            }

            Console.WriteLine($"[Cliente {id}] Desconectado");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Cliente {id}] Erro: {ex.Message}");
    }
}
