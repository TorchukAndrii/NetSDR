using System.Net;
using System.Net.Sockets;
using NetSdrCore.Contracts;
using NetSdrCore.Exceptions;
using NetSdrCore.Helpers;
using NetSdrCore.Contracts;
using NetSdrCore.Exceptions;
using NetSdrCore.Helpers;

namespace NetSdrCore;

public class TcpCommunicationClient : ITcpCommunicationClient
{
    private const int HeaderSize = 2;
    private readonly TcpClient _tcpClient;
    private NetworkStream? _stream;

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

        if (_stream.DataAvailable == false)
        {
            return Array.Empty<byte>();
        }
                
        var headerBytes = await ReadAsync(_stream!, HeaderSize);
        var header = MessageHeader.FromBytes(headerBytes);
        var payloadLength = header.Length - HeaderSize;

        var payload = payloadLength > 0
            ? await ReadAsync(_stream!, payloadLength)
            : Array.Empty<byte>();

        return CombineMessage(headerBytes, payload);
    }

    private static byte[] CombineMessage(byte[] header, byte[] payload)
    {
        var fullMessage = new byte[header.Length + payload.Length];
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
        if (IsConnected == false || _stream == null || _stream.CanWrite == false)
            throw new TcpCommunicationException("Client is not connected.");
    }
}