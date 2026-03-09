using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using RsaCrypto;

namespace RsaClientServer.Web.Services;

public class RsaClientService
{
    const int MaxLen = 240;
    TcpClient? _client;
    NetworkStream? _stream;
    string? _publicKey;

    public bool IsConnected => _client?.Connected == true;
    public string? PublicKey => _publicKey;
    public string? LastError { get; private set; }
    public ConcurrentBag<(string Plain, string Encrypted)> SentMessages { get; } = [];

    public event Action? OnStateChanged;

    public async Task<bool> ConnectAsync(string host, int port)
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();

            var lenBuf = new byte[4];
            if (await _stream.ReadAsync(lenBuf) < 4) { LastError = "Falha ao receber chave"; return false; }
            var len = BitConverter.ToInt32(lenBuf, 0);
            if (len <= 0 || len > 10000) { LastError = "Chave inválida"; return false; }

            var keyBuf = new byte[len];
            var total = 0;
            while (total < len)
            {
                var r = await _stream.ReadAsync(keyBuf.AsMemory(total, len - total));
                if (r == 0) break;
                total += r;
            }
            _publicKey = Encoding.UTF8.GetString(keyBuf, 0, total);
            LastError = null;
            OnStateChanged?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            OnStateChanged?.Invoke();
            return false;
        }
    }

    public async Task<bool> SendAsync(string text)
    {
        if (_stream == null || _publicKey == null || text.Length > MaxLen)
        {
            LastError = text.Length > MaxLen ? $"Máximo {MaxLen} caracteres" : "Não conectado";
            return false;
        }
        try
        {
            var encrypted = RsaService.Encrypt(text, _publicKey);
            var bytes = Encoding.UTF8.GetBytes(encrypted);
            await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length));
            await _stream.WriteAsync(bytes);
            SentMessages.Add((text, encrypted));
            OnStateChanged?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
    }

    public void Disconnect()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
        OnStateChanged?.Invoke();
    }
}
