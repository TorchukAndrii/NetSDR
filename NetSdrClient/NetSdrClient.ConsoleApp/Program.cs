using System.Net;
using System.Net.Sockets;
using NetSdrCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetSdrCore.Contracts;

internal class Program
{
    private static ILogger<Program> _logger;
    static async Task Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information))
            .AddSingleton<ITcpCommunicationClient, TcpCommunicationClient>()
            .AddSingleton<IUdpReceiver, UdpReceiver>()
            .AddSingleton<INetSdrClient, NetSdrClient>()
            .BuildServiceProvider();

        _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var client = serviceProvider.GetRequiredService<INetSdrClient>();

        _logger.LogInformation("Starting NetSDR Console Client...");

        var ip = "127.0.0.1";
        var port = 50000;
        var outputFilePath = "IQData.bin";
        ulong frequency = 145000000; // Example frequency in Hz (145 MHz)
        byte channelId = 0;

        try
        {
            var listener = new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();

            _logger.LogInformation("Connecting to receiver...");
            await client.ConnectAsync(ip, port);
            _logger.LogInformation("Connected!");

            _logger.LogInformation("Starting IQ data reception...");
            await client.StartReceivingIQAsync(outputFilePath);
            _logger.LogInformation($"Receiving IQ data. Writing to {outputFilePath}");

            _logger.LogInformation("Stopping IQ data reception...");
            await client.StopReceivingIQAsync();

            _logger.LogInformation($"Setting receiver frequency to {frequency / 1e6} MHz...");
            await client.SetFrequencyAsync(frequency, channelId);
            _logger.LogInformation("Frequency set!");

            _logger.LogInformation("Disconnecting...");
            await client.DisconnectAsync();
            _logger.LogInformation("Disconnected!");

            listener.Stop();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
        }
    }
}