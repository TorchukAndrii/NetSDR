using System.Net;
using NetSdrCore.Enums;
using NetSdrCore.CommandConfigurators;
using NetSdrCore.Contracts;
using NetSdrCore.Exceptions;
using Microsoft.Extensions.Logging;

namespace NetSdrCore;

public class NetSdrClient : INetSdrClient, IDisposable
{
    private readonly ITcpCommunicationClient _tcpCommunicationClient;
    private readonly IUdpReceiver _udpReceiver;
    private readonly ILogger<NetSdrClient> _logger;

    public NetSdrClient(
        ITcpCommunicationClient tcpCommunicationClient, 
        IUdpReceiver udpReceiver,
        ILogger<NetSdrClient> logger)
    {
        _udpReceiver = udpReceiver;
        _tcpCommunicationClient = tcpCommunicationClient;
        _logger = logger;
    }

    public void Dispose()
    {
        _tcpCommunicationClient.DisconnectAsync();
        _udpReceiver.Dispose();
    }

    public async Task ConnectAsync(string ip, int port = 50000)
    {
        if (IPAddress.TryParse(ip, out var address))
        {
            await _tcpCommunicationClient.ConnectAsync(address, port);
            _logger.LogInformation("Connected to {Ip}:{Port}.", ip, port);
            return;
        }

        _logger.LogError("Invalid IP address: {Ip}", ip);
        throw new ArgumentException("Invalid IP address.");
    }

    public async Task DisconnectAsync()
    {
        await StopReceivingIQAsync();
        await _tcpCommunicationClient.DisconnectAsync();
    }

    public async Task StartReceivingIQAsync(string filePath = "IQData.bin")
    {
        try
        {
            byte[] command = new ReceiverStateCommandConfigurator().SetStartCommand();
            await _tcpCommunicationClient.SendAsync(command);

            var response = await _tcpCommunicationClient.ReceiveAsync();
            ProcessResponse(response);

            _udpReceiver.StartReceiving(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while starting IQ data reception.");
            throw;
        }
    }
    
    public async Task StopReceivingIQAsync()
    {
        try
        {
            byte[] command = new ReceiverStateCommandConfigurator().SetStopCommand();
            await _tcpCommunicationClient.SendAsync(command);

            var response = await _tcpCommunicationClient.ReceiveAsync();
            ProcessResponse(response);

            await _udpReceiver.StopReceivingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while stopping IQ data reception.");
            throw;
        }
    }

    public async Task SetFrequencyAsync(ulong frequency, byte channelId)
    {
        try
        {
            byte[] command = new FrequencyCommandConfigurator().SetChannelId(channelId).SetFrequency(frequency);
            await _tcpCommunicationClient.SendAsync(command);

            var response = await _tcpCommunicationClient.ReceiveAsync();
            ProcessResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while setting frequency.");
            throw;
        }
    }

    private void ProcessResponse(byte[] response)
    {
        if (response.Length == 0)
        {
            _logger.LogWarning("Received an empty response from the receiver.");
            return; 
        }

        if (response.Length == 2 && BitConverter.ToUInt16(response, 0) == (ushort)ResponseType.NAK)
        {
            _logger.LogError("Received NAK response.");
            throw new NakException();
        }

        if (response.Length >= 3 && BitConverter.ToUInt16(response, 0) == (ushort)ResponseType.ACK)
        {
            _logger.LogInformation("Received ACK for Data Item {DataItem}.", response[2]);
            return;
        }

        if (response.Length > 4) 
        {
            _logger.LogWarning("Received Unsolicited Control Item - future processing needed.");
        }
    }
}
