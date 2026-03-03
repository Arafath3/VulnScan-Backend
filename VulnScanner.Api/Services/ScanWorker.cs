namespace VulnScanner.Api.Services;   // Services namespace

public sealed class ScanWorker : BackgroundService // BackgroundService runs continuously in host
{
    private readonly IBackgroundTaskQueue _queue;          // Queue to receive scan jobs
    private readonly IServiceScopeFactory _scopeFactory;   // Creates DI scopes inside worker

    public ScanWorker(IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory) // DI constructor
    {
        _queue = queue;                                    // Store queue
        _scopeFactory = scopeFactory;                      // Store scope factory
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) // Worker main loop
    {
        while (!stoppingToken.IsCancellationRequested)     // Loop until app shuts down
        {
            var request = await _queue.DequeueAsync(stoppingToken); // Wait for next scan job

            using var scope = _scopeFactory.CreateScope();          // Create a new DI scope for this job
            var runner = scope.ServiceProvider                      // Resolve services from this scope
                .GetRequiredService<ScanRunner>();                  // Get ScanRunner
            await runner.RunAsync(request.ScanRunId, stoppingToken); // Execute the scan
        }
    }
}