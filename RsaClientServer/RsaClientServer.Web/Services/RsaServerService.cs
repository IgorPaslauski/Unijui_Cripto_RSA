using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RsaCrypto;

namespace RsaClientServer.Web.Services;

public class RsaServerService : IDisposable
{
    const int Port = 5000;
    TcpListener? _listener;
    CancellationTokenSource? _cts;
    bool _running;
    int _clientId;

    public record Message(int ClientId, string Text, DateTime Time, string Endpoint, string CipherBase64);
    public ConcurrentBag<Message> Messages { get; } = [];
    public ConcurrentBag<string> ClientConnections { get; } = [];
    public bool IsRunning => _running;
    public string? PublicKey { get; private set; }

    public event Action? OnStateChanged;

    public void Start()
    {
        if (_running) return;
        var (publicKey, privateKey) = RsaService.GenerateKeyPair();
        PublicKey = publicKey;
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, Port);
        _listener.Start();
        _running = true;
        _ = RunLoop(privateKey, publicKey);
        OnStateChanged?.Invoke();
    }

    async Task RunLoop(string privateKey, string publicKey)
    {
        try
        {
            while (_cts?.Token.IsCancellationRequested == false && _listener != null)
            {
                var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                var id = Interlocked.Increment(ref _clientId);
                var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "?";
                ClientConnections.Add($"Cliente {id} ({endpoint})");
                OnStateChanged?.Invoke();
                _ = HandleClient(client, id, endpoint, publicKey, privateKey);
            }
        }
        catch (OperationCanceledException) { }
    }

    async Task HandleClient(TcpClient client, int id, string endpoint, string publicKey, string privateKey)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
                await stream.WriteAsync(BitConverter.GetBytes(publicKeyBytes.Length));
                await stream.WriteAsync(publicKeyBytes);

                while (true)
                {
                    var lenBuf = new byte[4];
                    if (await stream.ReadAsync(lenBuf) < 4) break;
                    var len = BitConverter.ToInt32(lenBuf, 0);
                    if (len <= 0 || len > 100_000) break;

                    var buf = new byte[len];
                    var total = 0;
                    while (total < len)
                    {
                        var r = await stream.ReadAsync(buf.AsMemory(total, len - total));
                        if (r == 0) break;
                        total += r;
                    }
                    if (total < len) break;

                    try
                    {
                        var cipherBase64 = Encoding.UTF8.GetString(buf, 0, total);
                        var decrypted = RsaService.Decrypt(cipherBase64, privateKey);
                        Messages.Add(new Message(id, decrypted, DateTime.Now, endpoint, cipherBase64));
                        OnStateChanged?.Invoke();
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _running = false;
        OnStateChanged?.Invoke();
    }

    public void Dispose() => Stop();
}
