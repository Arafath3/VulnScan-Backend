namespace VulnScanner.Api.Services; // Logical grouping for “service layer” classes

public sealed record ScanRequest(   // Small message type used for the queue
    Guid ScanRunId                  // The worker only needs the ScanRunId to do the job
);