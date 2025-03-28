using System.Net;
using NetSdrCore.Enums;
using NetSdrCore.CommandConfigurators;
using NetSdrCore.Contracts;
using NetSdrCore.Exceptions;

namespace NetSdrCore;

public class NetSdrClient : INetSdrClient, IDisposable
{
    private readonly ITcpCommunicationClient _tcpCommunicationClient;
    private readonly IUdpReceiver _udpReceiver;

    public NetSdrClient(ITcpCommunicationClient tcpCommunicationClient, IUdpReceiver udpReceiver)
    {
        _udpReceiver = udpReceiver;
        _tcpCommunicationClient = tcpCommunicationClient;
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
            return;
        }

        throw new ArgumentException("Invalid IP address.");
    }

    public async Task DisconnectAsync()
    {
        await StopReceivingIQAsync();
        await _tcpCommunicationClient.DisconnectAsync();
    }

    public async Task StartReceivingIQAsync(string filePath = "IQData.bin")
    {
        byte[] command = new ReceiverStateCommandConfigurator().SetStartCommand();
        await _tcpCommunicationClient.SendAsync(command);

        var response = await _tcpCommunicationClient.ReceiveAsync();
        ProcessResponse(response);

        _udpReceiver.StartReceiving(filePath);
    }
    
    public async Task StopReceivingIQAsync()
    {
        byte[] command = new ReceiverStateCommandConfigurator().SetStopCommand();
        await _tcpCommunicationClient.SendAsync(command);
        
        var response = await _tcpCommunicationClient.ReceiveAsync();
        ProcessResponse(response);

        await _udpReceiver.StopReceivingAsync();
    }

    public async Task SetFrequencyAsync(ulong frequency, byte channelId)
    {
        byte[] command = new FrequencyCommandConfigurator().SetChannelId(channelId).SetFrequency(frequency);
        await _tcpCommunicationClient.SendAsync(command);

        var response = await _tcpCommunicationClient.ReceiveAsync();
        ProcessResponse(response);
    }

    private void ProcessResponse(byte[] response)
    {
        if (response.Length == 2 && BitConverter.ToUInt16(response, 0) == (ushort)ResponseType.NAK)
        {
            throw new NakException();
        }
        
        if (response.Length >= 3 && BitConverter.ToUInt16(response, 0) == (ushort)ResponseType.ACK)
        {
            Console.WriteLine($"Received ACK for Data Item {response[2]}");
            return;
        }

        // Future support for Unsolicited Control Item messages
        if (response.Length > 4) 
        {
            Console.WriteLine("Received Unsolicited Control Item - future processing needed");
        }
    }
}
