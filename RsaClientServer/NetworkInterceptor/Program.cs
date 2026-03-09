using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

// Simulador de ataque — atua como "homem no meio" (MitM)
// Escuta o tráfego entre cliente e servidor e guarda logs
// NÃO conhece o protocolo; analisa dados brutos na rede
const int PortaInterceptor = 5001;
const int PortaAlvoPadrao = 5000;

var arquivoLog = $"interceptor_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
Logger.Init(arquivoLog);

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  FERRAMENTA DE ATAQUE - Interceptor de Comunicação      ║");
Console.WriteLine("║  (Simulação de invasor tentando ler o tráfego)           ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("O atacante posiciona-se entre cliente e servidor para");
Console.WriteLine("capturar e tentar decodificar a comunicação.");
Console.WriteLine();

Console.Write("IP do servidor alvo (Enter = 127.0.0.1): ");
var servidorAlvo = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(servidorAlvo))
    servidorAlvo = "127.0.0.1";

Console.Write($"Porta do servidor alvo (Enter = {PortaAlvoPadrao}): ");
var portaStr = Console.ReadLine()?.Trim();
var portaAlvo = int.TryParse(portaStr, out var p) ? p : PortaAlvoPadrao;

Console.WriteLine();
Console.WriteLine($"[ATAQUE] Escutando em 0.0.0.0:{PortaInterceptor} — aguardando vítimas");
Console.WriteLine($"[ATAQUE] Escutando servidor em {servidorAlvo}:{portaAlvo}");
Console.WriteLine($"[ATAQUE] A vítima (cliente) deve conectar em 127.0.0.1:{PortaInterceptor}");
Console.WriteLine($"[ATAQUE] Logs salvos em: {Path.GetFullPath(arquivoLog)}");
Console.WriteLine($"[ATAQUE] Ctrl+C para encerrar.");
Console.WriteLine();
Logger.Log($"Iniciado — escutando em :{PortaInterceptor}, servidor alvo {servidorAlvo}:{portaAlvo}");

var interceptor = new TcpListener(IPAddress.Any, PortaInterceptor);
interceptor.Start();

var sessaoId = 0;

try
{
    while (true)
    {
        var cliente = await interceptor.AcceptTcpClientAsync();
        var id = Interlocked.Increment(ref sessaoId);
        var endpointCliente = cliente.Client.RemoteEndPoint?.ToString() ?? "?";
        _ = Task.Run(async () => await InterceptarSessao(cliente, id, endpointCliente, servidorAlvo, portaAlvo));
    }
}
finally
{
    interceptor.Stop();
}

static async Task InterceptarSessao(TcpClient cliente, int id, string endpointCliente, string servidorAlvo, int portaAlvo)
{
    List<byte[]> dadosServidorParaCliente = [];
    List<byte[]> dadosClienteParaServidor = [];

    try
    {
        using var servidor = new TcpClient();
        await servidor.ConnectAsync(servidorAlvo, portaAlvo);

        using (cliente)
        using (var streamCliente = cliente.GetStream())
        using (servidor)
        using (var streamServidor = servidor.GetStream())
        {
            Escrever($"[CAPTURA] Sessão {id} | Alvo {endpointCliente} conectou. Interceptando...", ConsoleColor.Green);
            Logger.Log($"Sessão {id} | Alvo {endpointCliente} conectou. Escutando tráfego do servidor...");

            var cts = new CancellationTokenSource();

            var tarefaClienteParaServidor = Encaminhar(streamCliente, streamServidor, id, "Vítima→Servidor", dadosClienteParaServidor, cts.Token);
            var tarefaServidorParaCliente = Encaminhar(streamServidor, streamCliente, id, "Servidor→Vítima", dadosServidorParaCliente, cts.Token);

            await Task.WhenAny(tarefaClienteParaServidor, tarefaServidorParaCliente);
            cts.Cancel();
        }

        AnalisarDadosInterceptados(id, dadosServidorParaCliente, dadosClienteParaServidor);
    }
    catch (Exception ex)
    {
        Escrever($"[ATAQUE] Sessão {id} | Erro: {ex.Message}", ConsoleColor.Red);
        Logger.Log($"Sessão {id} | Erro: {ex.Message}");
    }
}

static async Task Encaminhar(NetworkStream origem, NetworkStream destino, int id, string direcao, List<byte[]> captura, CancellationToken ct)
{
    var buffer = new byte[65536];
    try
    {
        while (!ct.IsCancellationRequested)
        {
            var lido = await origem.ReadAsync(buffer, ct);
            if (lido <= 0) break;

            var copia = new byte[lido];
            Array.Copy(buffer, copia, lido);
            captura.Add(copia);

            await destino.WriteAsync(copia, ct);

            var dadosStr = Encoding.UTF8.GetString(copia);
            var logMsg = lido <= 200 ? $"{direcao} | {lido} bytes: {Truncar(dadosStr, 80)}" : $"{direcao} | {lido} bytes (dados sigilosos)";
            Logger.Log($"Sessão {id} | INTERCEPTADO | {logMsg}");
            if (lido <= 200)
                Escrever($"[INTERCEPTADO] Sessão {id} | {logMsg}", ConsoleColor.Yellow);
            else
                Escrever($"[INTERCEPTADO] Sessão {id} | {logMsg}", ConsoleColor.Yellow);
        }
    }
    catch (OperationCanceledException) { }
}

static void AnalisarDadosInterceptados(int id, List<byte[]> servidorParaCliente, List<byte[]> clienteParaServidor)
{
    Console.WriteLine();
    Escrever($"══════ Sessão {id} — Análise do que o atacante capturou ══════", ConsoleColor.Cyan);
    Logger.Log($"Sessão {id} — Análise dos dados capturados");

    var chaveInterceptada = ExtrairPossivelChave(servidorParaCliente);
    var mensagensInterceptadas = ExtrairMensagens(clienteParaServidor);

    if (chaveInterceptada != null)
    {
        EscreverSeparador("CAPTURADO (Servidor→Vítima) — Possível chave de criptografia");
        Console.WriteLine(chaveInterceptada);
        File.WriteAllText($"interceptado_chave_sessao{id}.txt", chaveInterceptada, Encoding.UTF8);
        Escrever($"[SALVO] interceptado_chave_sessao{id}.txt", ConsoleColor.DarkGray);
        Logger.Log($"Sessão {id} | Chave capturada salva em interceptado_chave_sessao{id}.txt");
    }

    for (var i = 0; i < mensagensInterceptadas.Count; i++)
    {
        var msg = mensagensInterceptadas[i];
        EscreverSeparador($"CAPTURADO (Vítima→Servidor) — Mensagem secreta #{i + 1}");
        Console.WriteLine(msg);
        File.WriteAllText($"interceptado_msg{i + 1}_sessao{id}.txt", msg, Encoding.UTF8);
        Escrever($"[SALVO] interceptado_msg{i + 1}_sessao{id}.txt", ConsoleColor.DarkGray);
        Logger.Log($"Sessão {id} | Mensagem #{i + 1} salva em interceptado_msg{i + 1}_sessao{id}.txt");

        if (chaveInterceptada != null)
            TentarDescriptografar(chaveInterceptada, msg);
    }

    EscreverSeparador("RESULTADO DO ATAQUE");
    Escrever("FALHOU: A chave capturada não descriptografa. A chave privada nunca trafega na rede.", ConsoleColor.Red);
    Logger.Log($"Sessão {id} | Resultado: falha ao descriptografar (chave pública)");
    Console.WriteLine();
}

static string? ExtrairPossivelChave(List<byte[]> pacotes)
{
    var buffer = pacotes.SelectMany(p => p).ToArray();
    if (buffer.Length < 4) return null;
    var length = BitConverter.ToInt32(buffer, 0);
    if (length > 0 && length <= buffer.Length - 4 && length < 50000)
    {
        var payload = Encoding.UTF8.GetString(buffer, 4, length);
        if (PareceBase64(payload) && payload.Length > 200)
            return payload;
    }
    return null;
}

static List<string> ExtrairMensagens(List<byte[]> pacotes)
{
    var resultado = new List<string>();
    var buffer = new List<byte>();
    foreach (var p in pacotes) buffer.AddRange(p);

    var offset = 0;
    while (offset + 4 <= buffer.Count)
    {
        var length = BitConverter.ToInt32([buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]], 0);
        offset += 4;
        if (length <= 0 || length > 200000 || offset + length > buffer.Count) break;

        var payload = Encoding.UTF8.GetString(buffer.Skip(offset).Take(length).ToArray());
        offset += length;
        if (PareceBase64(payload))
            resultado.Add(payload);
    }
    return resultado;
}

static bool PareceBase64(string s)
{
    if (string.IsNullOrWhiteSpace(s)) return false;
    return s.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=');
}

static void TentarDescriptografar(string chaveBase64, string cifraBase64)
{
    try
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(chaveBase64), out _);
        var decrypted = rsa.Decrypt(Convert.FromBase64String(cifraBase64), RSAEncryptionPadding.Pkcs1);
        var texto = Encoding.UTF8.GetString(decrypted);
        Escrever($"[ATAQUE OK] Conseguiu ler: {texto}", ConsoleColor.Green);
        Logger.Log($"[ATAQUE OK] Conseguiu ler: {texto}");
    }
    catch (CryptographicException)
    {
        Escrever("[FALHOU] Não foi possível ler a mensagem — chave capturada é só para criptografar.", ConsoleColor.Red);
        Logger.Log("[FALHOU] Tentativa de descriptografar com chave pública");
    }
}

static void Escrever(string msg, ConsoleColor cor)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = cor;
    Console.WriteLine(msg);
    Console.ForegroundColor = orig;
}

static void EscreverSeparador(string texto)
{
    Escrever($"─── {texto} ───", ConsoleColor.DarkGray);
}

static string Truncar(string s, int max) => s.Length <= max ? s : s[..max] + "...";

static class Logger
{
    static string? _arquivo;
    static readonly object Lock = new();
    public static void Init(string arquivo) => _arquivo = arquivo;
    public static void Log(string msg)
    {
        if (string.IsNullOrEmpty(_arquivo)) return;
        lock (Lock)
        {
            try { File.AppendAllText(_arquivo, $"[{DateTime.Now:HH:mm:ss}] {msg}" + Environment.NewLine, Encoding.UTF8); } catch { }
        }
    }
}
