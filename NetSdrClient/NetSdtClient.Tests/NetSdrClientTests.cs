using System.Net;
using Moq;
using NetSdrCore.Contracts;
using NetSdrCore;
using NetSdrCore.Exceptions;

namespace NetSdtCore.Tests;

public class NetSdrClientTests
{
    [Fact]
    public async Task ConnectAsync_ValidIp_EstablishesConnection()
    {
        var tcpClientMock = new Mock<ITcpCommunicationClient>();
        var udpReceiverMock = new Mock<IUdpReceiver>();
        var client = new NetSdrClient(tcpClientMock.Object, udpReceiverMock.Object);

        await client.ConnectAsync("127.0.0.1");
        tcpClientMock.Verify(c => c.ConnectAsync(It.IsAny<IPAddress>(), 50000), Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_CallsDisconnectOnClients()
    {
        var tcpClientMock = new Mock<ITcpCommunicationClient>();
        var udpReceiverMock = new Mock<IUdpReceiver>();
        var client = new NetSdrClient(tcpClientMock.Object, udpReceiverMock.Object);

        await client.DisconnectAsync();
        tcpClientMock.Verify(c => c.DisconnectAsync(), Times.Once);
        udpReceiverMock.Verify(c => c.StopReceivingAsync(), Times.Once);
    }
    
    [Fact]
    public async Task SetFrequencyAsync_ReceivesNAK_ThrowsNAKException()
    {
        var tcpClientMock = new Mock<ITcpCommunicationClient>();
        var udpReceiverMock = new Mock<IUdpReceiver>();
        tcpClientMock.Setup(c => c.ReceiveAsync()).ReturnsAsync(new byte[] { 0x00, 0x02 });
        var client = new NetSdrClient(tcpClientMock.Object, udpReceiverMock.Object);

        await Assert.ThrowsAsync<NakException>(async () => await client.SetFrequencyAsync(1000000, 1));
    }
}