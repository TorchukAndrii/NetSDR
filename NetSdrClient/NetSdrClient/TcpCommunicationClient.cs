using NetSdrClient.Exceptions;

namespace NetSdrClient;


using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

    public class TcpCommunicationClient : ITcpCommunicationClient
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream? _stream;
        
        public bool IsConnected { get; private set; }


        public TcpCommunicationClient()
        {
            _tcpClient = new TcpClient();
        }

        
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

                if(_stream != null)
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
            try
            {
                if (_stream == null || !_stream.CanWrite)
                    throw new TcpCommunicationException("Failed to connect");

                await _stream.WriteAsync(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public async Task<byte[]> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

    }
