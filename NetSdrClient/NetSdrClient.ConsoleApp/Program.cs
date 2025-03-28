using NetSdrClient;

Console.WriteLine("Starting NetSDR Console Client...");

var ip = "127.0.0.1"; // Change to your receiver's IP
var port = 50000;
var outputFilePath = "IQData.bin";
ulong frequency = 145000000; // Example frequency in Hz (145 MHz)
byte channelId = 0;

using var client = new NetSdrClient.NetSdrClient(new TcpCommunicationClient(), new UdpReceiver());

try
{
    Console.WriteLine("Connecting to receiver...");
    await client.ConnectAsync(ip, port);
    Console.WriteLine("Connected!");

    Console.WriteLine("Starting IQ data reception...");
    await client.StartReceivingIQAsync(outputFilePath);
    Console.WriteLine($"Receiving IQ data. Writing to {outputFilePath}");

    await Task.Delay(5000); // Simulating 5 seconds of data reception

    Console.WriteLine("Stopping IQ data reception...");
    await client.StopReceivingIQAsync();

    Console.WriteLine($"Setting receiver frequency to {frequency / 1e6} MHz...");
    await client.SetFrequencyAsync(frequency, channelId);
    Console.WriteLine("Frequency set!");

    Console.WriteLine("Disconnecting...");
    await client.DisconnectAsync();
    Console.WriteLine("Disconnected!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}