using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetSdrCore;
using NetSdrCore.Contracts;
using NetSdrCore.Exceptions;
using Xunit;

namespace NetSdtCore.Tests;
public class NetSdrClientTests
{
    private readonly Mock<ITcpCommunicationClient> _tcpClientMock;
    private readonly Mock<IUdpReceiver> _udpReceiverMock;
    private readonly Mock<ILogger<NetSdrClient>> _loggerMock;
    private readonly NetSdrClient _client;

    public NetSdrClientTests()
    {
        _tcpClientMock = new Mock<ITcpCommunicationClient>();
        _udpReceiverMock = new Mock<IUdpReceiver>();
        _loggerMock = new Mock<ILogger<NetSdrClient>>();
        _client = new NetSdrClient(_tcpClientMock.Object, _udpReceiverMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ConnectAsync_ValidIp_EstablishesConnection()
    {
        await _client.ConnectAsync("127.0.0.1");

        _tcpClientMock.Verify(c => c.ConnectAsync(It.IsAny<IPAddress>(), 50000), Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_CallsDisconnectOnClients()
    {
        await _client.DisconnectAsync();

        _tcpClientMock.Verify(c => c.DisconnectAsync(), Times.Once);
        _udpReceiverMock.Verify(c => c.StopReceivingAsync(), Times.Once);

    }
    
    [Fact]
    public async Task SetFrequencyAsync_ReceivesNAK_ThrowsNAKException()
    {
        _tcpClientMock.Setup(c => c.ReceiveAsync()).ReturnsAsync(new byte[] { 0x00, 0x02 });

        await Assert.ThrowsAsync<NakException>(async () => await _client.SetFrequencyAsync(1000000, 1));
    }
}
