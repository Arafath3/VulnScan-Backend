namespace VulnScanner.Api.Services; // Same namespace as other services

public interface IBackgroundTaskQueue // Defines what a queue must be able to do
{
    ValueTask EnqueueAsync(             // Adds a scan request into the queue
        ScanRequest request,            // The request payload
        CancellationToken ct            // Cancellation token for shutdown or request cancel
    );

    ValueTask<ScanRequest> DequeueAsync( // Removes the next scan request from the queue
        CancellationToken ct             // Cancellation token for shutdown
    );
}