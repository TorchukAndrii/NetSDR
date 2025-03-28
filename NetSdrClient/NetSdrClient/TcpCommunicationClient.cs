using System.Net;
using System.Net.Sockets;
using NetSdrClient.Contracts;
using NetSdrClient.Exceptions;
using NetSdrClient.Helpers;

namespace NetSdrClient;

public class TcpCommunicationClient : ITcpCommunicationClient
{
    private readonly TcpClient _tcpClient;
    private NetworkStream? _stream;
    private const int HeaderSize = 2;

    public TcpCommunicationClient()
    {
        _tcpClient = new TcpClient();
    }

    public bool IsConnected { get; private set; }


    public async Task ConnectAsync(IPAddress host, int port = 50000)
    {
        try
        {
            await _tcpClient.ConnectAsync(host, port);

            _stream = _tcpClient.GetStream();

            IsConnected = true;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            throw new TcpCommunicationException($"Failed to Connect: {ex.Message}", ex);
        }
    }


    public async Task DisconnectAsync()
    {
        try
        {
            if (!IsConnected)
                return;

            if (_stream != null)
            {
                _stream.Close();
                await _stream.DisposeAsync();
            }

            _tcpClient.Client.Close();
            _tcpClient.Close();
            _tcpClient.Dispose();

            IsConnected = false;

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new TcpCommunicationException("Failed to disconnect: ", ex);
        }
    }


    public async Task SendAsync(byte[] data)
    {
        EnsureConnected();

        await _stream!.WriteAsync(data);
    }


    public async Task<byte[]> ReceiveAsync()
    {
        EnsureConnected();
        
        var headerBytes = await ReadAsync(_stream!, HeaderSize);
        var header = MessageHeader.FromBytes(headerBytes);
        int payloadLength = header.Length - HeaderSize;

        byte[] payload = payloadLength > 0
            ? await ReadAsync(_stream!, payloadLength)
            : Array.Empty<byte>();

        return CombineMessage(headerBytes, payload);
    }
    
    private static byte[] CombineMessage(byte[] header, byte[] payload)
    {
        byte[] fullMessage = new byte[header.Length + payload.Length];
        Array.Copy(header, 0, fullMessage, 0, header.Length);
        Array.Copy(payload, 0, fullMessage, header.Length, payload.Length);
        return fullMessage;
    }
    private static async Task<byte[]> ReadAsync(NetworkStream stream, int count)
    {
        var buffer = new byte[count];
        var offset = 0;
        while (offset < count)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, count - offset));
            if (bytesRead == 0) throw new EndOfStreamException();
            offset += bytesRead;
        }

        return buffer;
    }
    private void EnsureConnected()
    {
        if (IsConnected || _stream == null || !_stream.CanWrite)
            throw new TcpCommunicationException("Client is not connected.");
    }
}