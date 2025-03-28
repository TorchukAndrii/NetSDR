namespace NetSdrCore.Contracts;

public interface INetSdrClient
{
    void Dispose();
    Task ConnectAsync(string ip, int port = 50000);
    Task DisconnectAsync();
    Task StartReceivingIQAsync(string filePath = "IQData.bin");
    Task StopReceivingIQAsync();
    Task SetFrequencyAsync(ulong frequency, byte channelId);
}