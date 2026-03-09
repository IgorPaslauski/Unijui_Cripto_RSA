using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using RsaCrypto;

const int Port = 5000;
var mostrarChavePrivada = args.Contains("--debug");
var endereco = args.Where(a => !a.StartsWith("--")).FirstOrDefault() is { } arg ? IPAddress.Parse(arg) : IPAddress.Any;
var clientId = 0;
var cts = new CancellationTokenSource();
var versao = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0";

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

EscreverCabecalho($"SERVIDOR RSA v{versao} - Segurança de Dados");
Console.WriteLine();

var (publicKey, privateKey) = RsaService.GenerateKeyPair();
EscreverSucesso("Par de chaves RSA gerado (2048 bits)");
if (mostrarChavePrivada)
{
    EscreverSeparador("CHAVE PRIVADA (apenas para demonstração - NUNCA exponha em produção)");
    EscreverCifra(privateKey);
    EscreverSeparador(null);
}
Console.WriteLine();

var listener = new TcpListener(endereco, Port);
listener.Start();
EscreverSucesso($"Servidor escutando em {endereco}:{Port}");
EscreverInfo("Aguardando conexões (Ctrl+C para encerrar)...");
Console.WriteLine();

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var client = await listener.AcceptTcpClientAsync(cts.Token);
        var id = Interlocked.Increment(ref clientId);
        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "?";
        _ = Task.Run(async () => await TratarCliente(client, id, endpoint, publicKey, privateKey));
    }
}
catch (OperationCanceledException) { }
catch (Exception ex)
{
    EscreverErro(ex.Message);
}
finally
{
    listener.Stop();
    Console.WriteLine();
    EscreverInfo("Servidor encerrado. Pressione qualquer tecla para sair.");
    Console.ReadKey();
}

static async Task TratarCliente(TcpClient client, int id, string endpoint, string publicKey, string privateKey)
{
    try
    {
        using (client)
        using (var stream = client.GetStream())
        {
            EscreverSucesso($"[Cliente {id}] Conectado de {endpoint}");

            var publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
            var lengthBytes = BitConverter.GetBytes(publicKeyBytes.Length);
            await stream.WriteAsync(lengthBytes);
            await stream.WriteAsync(publicKeyBytes);

            EscreverInfo($"[Cliente {id}] Chave pública enviada ao cliente");
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

                try
                {
                    var decryptedMessage = RsaService.Decrypt(encryptedBase64, privateKey);
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    EscreverMensagem($"[Cliente {id}] [{timestamp}] {decryptedMessage}");
                }
                catch (CryptographicException)
                {
                    EscreverErro($"[Cliente {id}] Mensagem recebida inválida ou adulterada — não foi possível descriptografar.");
                }
            }

            EscreverInfo($"[Cliente {id}] Desconectado");
        }
    }
    catch (Exception ex)
    {
        EscreverErro($"[Cliente {id}] {ex.Message}");
    }
}

static void EscreverCabecalho(string titulo)
{
    var largura = Math.Max(50, titulo.Length + 4);
    var linha = new string('═', largura);
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"╔{linha}╗");
    Console.WriteLine($"║  {titulo.PadRight(largura - 2)}║");
    Console.WriteLine($"╚{linha}╝");
    Console.ForegroundColor = orig;
}

static void EscreverSeparador(string? texto)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    if (!string.IsNullOrEmpty(texto))
        Console.WriteLine($"─── {texto} ───");
    else
        Console.WriteLine("─".PadRight(60, '─'));
    Console.ForegroundColor = orig;
}

static void EscreverSucesso(string msg)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[OK] {msg}");
    Console.ForegroundColor = orig;
}

static void EscreverErro(string msg)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERRO] {msg}");
    Console.ForegroundColor = orig;
}

static void EscreverInfo(string msg)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[...] {msg}");
    Console.ForegroundColor = orig;
}

static void EscreverMensagem(string msg)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(msg);
    Console.ForegroundColor = orig;
}

static void EscreverCifra(string texto)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(texto);
    Console.ForegroundColor = orig;
}
