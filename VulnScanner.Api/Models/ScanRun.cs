namespace VulnScanner.Api.Models;                   // Namespace

public enum ScanStatus                              // Scan lifecycle states
{
    Queued,                                         // Created but not processed yet
    Running,                                        // Worker is currently scanning
    Finished,                                       // Completed successfully
    Failed                                          // Completed with an error
}

public sealed class ScanRun                         // One execution instance of a scan
{
    public Guid Id { get; set; }                    // Primary key column in SQL (ScanRuns.Id)
    public Guid TargetId { get; set; }              // Foreign key to Targets.Id
    public ScanStatus Status { get; set; }          // Current status column
        = ScanStatus.Queued;                        // Default status
    public DateTimeOffset CreatedAt { get; set; }   // When this scan run was created
        = DateTimeOffset.UtcNow;                    // Default now
    public DateTimeOffset? StartedAt { get; set; }  // Nullable, filled when scan starts
    public DateTimeOffset? FinishedAt { get; set; } // Nullable, filled when scan ends
    public string? Error { get; set; }              // Nullable, stores error message if failed
}