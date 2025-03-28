using System.Net;
using Moq;
using NetSdrClient.Contracts;
using NetSdrClient.Exceptions;

namespace NetSdtClient.Tests;

public class NetSdrClientTests
{
    [Fact]
    public async Task ConnectAsync_ValidIp_EstablishesConnection()
    {
        var tcpClientMock = new Mock<ITcpCommunicationClient>();
        var udpReceiverMock = new Mock<IUdpReceiver>();
        var client = new NetSdrClient.NetSdrClient(tcpClientMock.Object, udpReceiverMock.Object);

        await client.ConnectAsync("127.0.0.1", 50000);
        tcpClientMock.Verify(c => c.ConnectAsync(It.IsAny<IPAddress>(), 50000), Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_CallsDisconnectOnClients()
    {
        var tcpClientMock = new Mock<ITcpCommunicationClient>();
        var udpReceiverMock = new Mock<IUdpReceiver>();
        var client = new NetSdrClient.NetSdrClient(tcpClientMock.Object, udpReceiverMock.Object);

        await client.DisconnectAsync();
        tcpClientMock.Verify(c => c.DisconnectAsync(), Times.Once);
        udpReceiverMock.Verify(c => c.StopReceivingAsync(), Times.Once);
    }
}