using System.Threading.Channels;       // Channels provide a built-in async queue

namespace VulnScanner.Api.Services;    // Services namespace

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue // Implements the queue interface
{
    private readonly Channel<ScanRequest> _channel;            // Channel holds ScanRequest items

    public BackgroundTaskQueue()                               // Constructor runs when app starts
    {
        _channel = Channel.CreateBounded<ScanRequest>(         // Bounded prevents unlimited memory growth
            new BoundedChannelOptions(100)                     // Max 100 pending scan jobs
            {
                SingleReader = true,                           // One worker thread reads
                SingleWriter = false,                          // Many HTTP requests can write
                FullMode = BoundedChannelFullMode.Wait         // If full, writers wait for space
            }
        );
    }

    public ValueTask EnqueueAsync(ScanRequest request, CancellationToken ct) // Enqueue a request
        => _channel.Writer.WriteAsync(request, ct);                           // Write into channel

    public ValueTask<ScanRequest> DequeueAsync(CancellationToken ct)          // Dequeue next request
        => _channel.Reader.ReadAsync(ct);                                     // Read, waits if empty
}