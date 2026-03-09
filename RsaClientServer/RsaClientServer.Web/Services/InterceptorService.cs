using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace RsaClientServer.Web.Services;

public class InterceptorService : IDisposable
{
    const int PortInterceptor = 5001;
    const int PortTarget = 5000;
    TcpListener? _listener;
    CancellationTokenSource? _cts;
    bool _running;

    public record CapturedItem(string Direction, string Data, int Bytes, DateTime Time);
    public record DecryptAttempt(string CipherBase64, bool Success, string? Message);

    public ConcurrentBag<CapturedItem> Captured { get; } = [];
    public string? CapturedKey { get; private set; }
    public List<DecryptAttempt> DecryptAttempts { get; } = [];

    public bool IsRunning => _running;
    public string TargetServer { get; set; } = "127.0.0.1";
    public int TargetPort { get; set; } = PortTarget;

    public event Action? OnStateChanged;

    public void Start()
    {
        if (_running) return;
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, PortInterceptor);
        _listener.Start();
        _running = true;
        CapturedKey = null;
        DecryptAttempts.Clear();
        Captured.Clear();
        _ = AcceptLoop();
        OnStateChanged?.Invoke();
    }

    async Task AcceptLoop()
    {
        while (_cts?.Token.IsCancellationRequested == false && _listener != null)
        {
            var client = await _listener.AcceptTcpClientAsync(_cts.Token);
            _ = HandleSession(client);
        }
    }

    async Task HandleSession(TcpClient client)
    {
        var toClient = new List<byte[]>();
        var toServer = new List<byte[]>();

        using (client)
        using (var server = new TcpClient())
        {
            try
            {
                await server.ConnectAsync(TargetServer, TargetPort);
            }
            catch
            {
                return;
            }

            using (var streamServer = server.GetStream())
            using (var streamClient = client.GetStream())
            {
                var cts = new CancellationTokenSource();
                var t1 = Relay(streamServer, streamClient, toClient, "Servidor→Vítima");
                var t2 = Relay(streamClient, streamServer, toServer, "Vítima→Servidor");
                await Task.WhenAny(t1, t2);
                cts.Cancel();
            }
        }

        Analyze(toClient, toServer);
        OnStateChanged?.Invoke();
    }

    async Task Relay(NetworkStream from, NetworkStream to, List<byte[]> capture, string dir)
    {
        var buf = new byte[65536];
        try
        {
            while (true)
            {
                var n = await from.ReadAsync(buf);
                if (n <= 0) break;
                var copy = buf[..n].ToArray();
                capture.Add(copy);
                string data;
                try
                {
                    data = Encoding.UTF8.GetString(copy);
                }
                catch { data = $"[{n} bytes binários]"; }
                Captured.Add(new CapturedItem(dir, data, n, DateTime.Now));
                await to.WriteAsync(copy);
                OnStateChanged?.Invoke();
            }
        }
        catch { }
    }

    void Analyze(List<byte[]> fromServer, List<byte[]> fromClient)
    {
        var all = fromServer.SelectMany(x => x).ToArray();
        if (all.Length >= 4)
        {
            var len = BitConverter.ToInt32(all, 0);
            if (len > 0 && len <= all.Length - 4)
            {
                var payload = Encoding.UTF8.GetString(all, 4, len);
                if (IsBase64(payload) && payload.Length > 200)
                {
                    CapturedKey = payload;
                }
            }
        }

        var clientBuf = fromClient.SelectMany(x => x).ToList();
        var offset = 0;
        while (offset + 4 <= clientBuf.Count)
        {
            var len = BitConverter.ToInt32([clientBuf[offset], clientBuf[offset + 1], clientBuf[offset + 2], clientBuf[offset + 3]], 0);
            offset += 4;
            if (len <= 0 || len > 200000 || offset + len > clientBuf.Count) break;
            var cipher = Encoding.UTF8.GetString(clientBuf.Skip(offset).Take(len).ToArray());
            offset += len;
            if (IsBase64(cipher) && CapturedKey != null)
            {
                try
                {
                    using var rsa = RSA.Create();
                    rsa.ImportRSAPublicKey(Convert.FromBase64String(CapturedKey), out _);
                    var dec = rsa.Decrypt(Convert.FromBase64String(cipher), RSAEncryptionPadding.Pkcs1);
                    DecryptAttempts.Add(new DecryptAttempt(cipher, true, Encoding.UTF8.GetString(dec)));
                }
                catch (CryptographicException)
                {
                    DecryptAttempts.Add(new DecryptAttempt(cipher, false, null));
                }
            }
        }
    }

    static bool IsBase64(string s) => s.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=');

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _running = false;
        OnStateChanged?.Invoke();
    }

    public void Dispose() => Stop();
}
