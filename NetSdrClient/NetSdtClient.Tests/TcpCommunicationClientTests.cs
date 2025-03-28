using System.Net;
using System.Net.Sockets;
using System.Text;
using NetSdrClient;
using NetSdrClient.Exceptions;

namespace NetSdtClient.Tests;

public class TcpCommunicationClientTests
{
    private const string Localhost = "127.0.0.1";

    private int GetRandomPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public async Task ConnectAsync_EstablishesConnection()
    {
        var testPort = GetRandomPort();
        var listener = new TcpListener(IPAddress.Parse(Localhost), testPort);
        listener.Start();

        var client = new TcpCommunicationClient();
        await client.ConnectAsync(IPAddress.Parse(Localhost), testPort);

        Assert.True(client.IsConnected);

        listener.Stop();
    }

    [Fact]
    public async Task DisconnectAsync_ClosesConnection()
    {
        var testPort = GetRandomPort();
        var listener = new TcpListener(IPAddress.Parse(Localhost), testPort);
        listener.Start();

        var client = new TcpCommunicationClient();
        await client.ConnectAsync(IPAddress.Parse(Localhost), testPort);
        await client.DisconnectAsync();

        Assert.False(client.IsConnected);

        listener.Stop();
    }

    [Fact]
    public async Task SendAsync_ThrowsException_WhenNotConnected()
    {
        var client = new TcpCommunicationClient();
        await Assert.ThrowsAsync<TcpCommunicationException>(async () =>
            await client.SendAsync(Encoding.UTF8.GetBytes("Hello")));
    }

    [Fact]
    public async Task ReceiveAsync_ThrowsException_WhenNotConnected()
    {
        var client = new TcpCommunicationClient();
        await Assert.ThrowsAsync<TcpCommunicationException>(async () => await client.ReceiveAsync());
    }
}