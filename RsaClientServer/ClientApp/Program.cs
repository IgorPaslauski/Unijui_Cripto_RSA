using System.Net.Sockets;
using System.Reflection;
using System.Text;
using RsaCrypto;

const int PortaPadrao = 5000;
const int MaxMensagemChars = 240;

var versao = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0";
EscreverCabecalho($"CLIENTE RSA v{versao} - Segurança de Dados");
Console.WriteLine();

Console.Write("Digite o IP do servidor (Enter para localhost): ");
var host = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(host))
    host = "127.0.0.1";

Console.Write($"Digite a porta (Enter para {PortaPadrao}): ");
var portaStr = Console.ReadLine()?.Trim();
var porta = int.TryParse(portaStr, out var p) ? p : PortaPadrao;

try
{
    using var client = new TcpClient();
    EscreverInfo($"[1/4] Conectando ao servidor em {host}:{porta}...");
    await client.ConnectAsync(host, porta);
    EscreverSucesso("[2/4] Conectado ao servidor!");
    Console.WriteLine();

    using var stream = client.GetStream();

    var lengthBuffer = new byte[4];
    var bytesRead = await stream.ReadAsync(lengthBuffer);
    if (bytesRead < 4)
    {
        EscreverErro("Não foi possível receber a chave pública.");
        return;
    }

    var keyLength = BitConverter.ToInt32(lengthBuffer, 0);
    if (keyLength <= 0 || keyLength > 10_000)
    {
        EscreverErro("Tamanho de chave inválido.");
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
    EscreverSucesso("[3/4] Chave pública recebida do servidor.");
    Console.WriteLine();
    EscreverSeparador("CHAVE PÚBLICA RECEBIDA (interceptável na rede - copie para análise)");
    EscreverCifra(publicKey);
    EscreverSeparador(null);
    Console.WriteLine();
    MostrarAjuda();
    EscreverInfo("[4/4] Digite suas mensagens (ou 'sair' para encerrar):");
    Console.WriteLine();

    string? ultimaCifra = null;
    var mensagensEnviadas = 0;

    while (true)
    {
        Console.Write("Mensagem: ");
        var message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message))
            continue;

        if (message.Equals("sair", StringComparison.OrdinalIgnoreCase))
        {
            EscreverInfo("Encerrando conexão...");
            break;
        }

        if (message.Equals("ajuda", StringComparison.OrdinalIgnoreCase) || message == "?")
        {
            MostrarAjuda();
            continue;
        }

        if (message.Equals("info", StringComparison.OrdinalIgnoreCase))
        {
            MostrarInfo(host, porta, publicKey.Length, MaxMensagemChars, mensagensEnviadas);
            continue;
        }

        if (message.Equals("chave", StringComparison.OrdinalIgnoreCase))
        {
            SalvarChave(publicKey);
            continue;
        }

        if (message.Equals("exportar", StringComparison.OrdinalIgnoreCase))
        {
            SalvarCifra(ultimaCifra);
            continue;
        }

        if (message.Length > MaxMensagemChars)
        {
            EscreverErro($"Mensagem muito longa. Use no máximo {MaxMensagemChars} caracteres.");
            continue;
        }

        var encryptedBase64 = RsaService.Encrypt(message, publicKey);
        ultimaCifra = encryptedBase64;
        mensagensEnviadas++;

        var encryptedBytes = Encoding.UTF8.GetBytes(encryptedBase64);
        var lengthBytes = BitConverter.GetBytes(encryptedBytes.Length);
        await stream.WriteAsync(lengthBytes);
        await stream.WriteAsync(encryptedBytes);
        EscreverSucesso("Mensagem enviada!");
        EscreverInfo("Texto cifrado enviado (interceptável na rede - copie para análise):");
        EscreverCifra(encryptedBase64);
    }

    Console.WriteLine();
    EscreverInfo("Pressione qualquer tecla para sair.");
    Console.ReadKey();
}
catch (SocketException ex)
{
    EscreverErro($"Conexão: {ex.Message}");
    EscreverInfo("Certifique-se de que o servidor está rodando antes de iniciar o cliente.");
}
catch (Exception ex)
{
    EscreverErro(ex.Message);
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

static void EscreverCifra(string texto)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(texto);
    Console.ForegroundColor = orig;
}

static void MostrarAjuda()
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("  Comandos: sair | ajuda ou ? | info | chave | exportar");
    Console.WriteLine("  - info     : dados da conexão e limites");
    Console.WriteLine("  - chave    : salva a chave pública em chave_publica.txt");
    Console.WriteLine("  - exportar : salva a última cifra enviada em ultima_cifra.txt");
    Console.ForegroundColor = orig;
}

static void MostrarInfo(string host, int porta, int tamanhoChave, int maxChars, int mensagensEnviadas)
{
    EscreverSeparador("INFORMAÇÕES DA CONEXÃO");
    EscreverInfo($"  Servidor        : {host}:{porta}");
    EscreverInfo($"  Chave pública   : {tamanhoChave} caracteres Base64");
    EscreverInfo($"  Limite mensagem : {maxChars} caracteres");
    EscreverInfo($"  Mensagens enviadas : {mensagensEnviadas}");
    EscreverSeparador(null);
}

static void SalvarChave(string publicKey)
{
    try
    {
        var arquivo = "chave_publica.txt";
        File.WriteAllText(arquivo, publicKey, Encoding.UTF8);
        EscreverSucesso($"Chave pública salva em {Path.GetFullPath(arquivo)}");
    }
    catch (Exception ex)
    {
        EscreverErro($"Não foi possível salvar: {ex.Message}");
    }
}

static void SalvarCifra(string? cifra)
{
    if (string.IsNullOrEmpty(cifra))
    {
        EscreverErro("Nenhuma mensagem foi enviada ainda. Envie uma mensagem antes de usar 'exportar'.");
        return;
    }
    try
    {
        var arquivo = "ultima_cifra.txt";
        File.WriteAllText(arquivo, cifra, Encoding.UTF8);
        EscreverSucesso($"Cifra salva em {Path.GetFullPath(arquivo)}");
    }
    catch (Exception ex)
    {
        EscreverErro($"Não foi possível salvar: {ex.Message}");
    }
}
