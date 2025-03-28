using System.Net.Sockets;
using System.Text;
using NetSdrClient;

namespace NetSdtClient.Tests;

public class UdpReceiverTests
{
    private const string TestFilePath = "test_output.dat";
    private const string Localhost = "127.0.0.1";

    private int GetRandomPort()
    {
        return new Random().Next(49152, 65535);
    }

    [Fact]
    public async Task StartReceiving_ReceivesUdpDataAndWritesToFile()
    {
        var testPort = GetRandomPort();
        if (File.Exists(TestFilePath))
            File.Delete(TestFilePath);

        using var udpReceiver = new UdpReceiver(testPort);
        udpReceiver.StartReceiving(TestFilePath);

        using var udpClient = new UdpClient();
        udpClient.Connect(Localhost, testPort);

        var testData = Encoding.UTF8.GetBytes("Test UDP data");
        await udpClient.SendAsync(testData, testData.Length);

        await Task.Delay(500);

        await udpReceiver.StopReceivingAsync();

        Assert.True(File.Exists(TestFilePath));
        var fileContent = await File.ReadAllBytesAsync(TestFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task StopReceivingAsync_StopsReceiverGracefully()
    {
        var testPort = GetRandomPort();
        if (File.Exists(TestFilePath))
            File.Delete(TestFilePath);

        using var udpReceiver = new UdpReceiver(testPort);
        udpReceiver.StartReceiving(TestFilePath);

        using var udpClient = new UdpClient();
        udpClient.Connect(Localhost, testPort);

        var testData = Encoding.UTF8.GetBytes("Test UDP data");
        await udpClient.SendAsync(testData, testData.Length);
        await Task.Delay(500); // Allow time for processing

        var initialFileSize = new FileInfo(TestFilePath).Length;

        // Act
        await udpReceiver.StopReceivingAsync();

        await udpClient.SendAsync(testData, testData.Length);
        await Task.Delay(500); // Ensure no more data is written

        var finalFileSize = new FileInfo(TestFilePath).Length;

        // Assert
        Assert.Equal(initialFileSize, finalFileSize); // File size should not change
    }
}