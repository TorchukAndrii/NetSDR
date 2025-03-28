using System.Net.Sockets;
using System.Threading.Channels;
using NetSdrClient.Contracts;

namespace NetSdrClient;

public class UdpReceiver : IUdpReceiver, IDisposable
{
    private readonly int _port;
    private Task? _backgroundTask;
    private CancellationTokenSource _cts = new();
    private bool _disposed;
    private Channel<byte[]>? _udpChannel;
    private UdpClient? _udpClient;

    public UdpReceiver(int port = 60000)
    {
        _port = port;
    }

    public void StartReceiving(string outputFilePath)
    {
        if (_udpClient != null)
            throw new InvalidOperationException("UDP Receiver is already running.");
        
        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        _udpClient = new UdpClient(_port);
        _udpChannel = Channel.CreateUnbounded<byte[]>();

        _backgroundTask = Task.WhenAll(
            ReceiveUdpDataAsync(_cts.Token),
            ProcessUdpDataAsync(outputFilePath, _cts.Token)
        );
    }

    public async Task StopReceivingAsync()
    {
        if (_cts.IsCancellationRequested) return;

        _cts.Cancel();

        try
        {
            if (_backgroundTask != null)
                await _backgroundTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("UDP receiver cancellation requested.");
        }
        finally
        {
            _udpChannel?.Writer.TryComplete();
            _udpChannel = null;

            _udpClient?.Close();
            _udpClient = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _cts.Dispose();

        _udpChannel?.Writer.TryComplete();
        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;
    }

    private async Task ProcessUdpDataAsync(string outputFilePath, CancellationToken cancellationToken)
    {
        try
        {
            await using var fileStream =
                new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);

            await foreach (var buffer in _udpChannel!.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                await fileStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                await fileStream.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("File writing stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing UDP data to file: {ex.Message}");
        }
    }

    private async Task ReceiveUdpDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_udpClient == null) return;

                var result = await _udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                await _udpChannel!.Writer.WriteAsync(result.Buffer, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("UDP Receiver stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving UDP data: {ex.Message}");
        }
    }
}