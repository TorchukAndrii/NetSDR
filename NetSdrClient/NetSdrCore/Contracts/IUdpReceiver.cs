namespace NetSdrCore.Contracts;

public interface IUdpReceiver
{
    void StartReceiving(string outputFilePath);
    Task StopReceivingAsync();
    void Dispose();
}