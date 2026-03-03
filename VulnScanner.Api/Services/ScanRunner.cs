using Microsoft.EntityFrameworkCore;    // EF Core async query methods
using VulnScanner.Api.Data;            // AppDbContext
using VulnScanner.Api.Models;          // ScanRun, ScanStatus, Target, Finding

namespace VulnScanner.Api.Services;    // Services namespace

public sealed class ScanRunner         // Orchestrates a full ScanRun execution
{
    private readonly AppDbContext _db;                     // Database access
    private readonly IEnumerable<IScannerModule> _modules; // All registered modules

    public ScanRunner(AppDbContext db, IEnumerable<IScannerModule> modules) // DI provides db and modules
    {
        _db = db;                                          // Store DbContext
        _modules = modules;                                // Store module list
    }

    public async Task RunAsync(Guid scanRunId, CancellationToken ct) // Executes scan by id
    {
        var scan = await _db.ScanRuns                      // Query ScanRuns table
            .FirstOrDefaultAsync(x => x.Id == scanRunId, ct); // Find matching ScanRun

        if (scan is null)                                  // If ScanRun not found
            return;                                        // Nothing to do

        scan.Status = ScanStatus.Running;                  // Update status to Running
        scan.StartedAt = DateTimeOffset.UtcNow;            // Save start time
        scan.Error = null;                                 // Clear previous error
        await _db.SaveChangesAsync(ct);                    // Persist status update

        try
        {
            var target = await _db.Targets                 // Query Targets table
                .FirstOrDefaultAsync(x => x.Id == scan.TargetId, ct); // Load Target for this ScanRun

            if (target is null)                            // If target missing
                throw new InvalidOperationException("Target not found"); // Mark scan failed

            foreach (var module in _modules)               // Run every registered module
            {
                var findings = await module.RunAsync(      // Execute module
                    target,                                 // Target data
                    scanRunId,                              // ScanRun link
                    ct                                      // Cancellation token
                );

                if (findings.Count > 0)                    // If module produced findings
                {
                    _db.Findings.AddRange(findings);        // Stage inserts into Findings table
                    await _db.SaveChangesAsync(ct);         // Persist findings immediately
                }
            }

            scan.Status = ScanStatus.Finished;             // Mark scan as finished
            scan.FinishedAt = DateTimeOffset.UtcNow;        // Save finish time
            await _db.SaveChangesAsync(ct);                 // Persist final status
        }
        catch (Exception ex)
        {
            scan.Status = ScanStatus.Failed;               // Mark scan as failed
            scan.FinishedAt = DateTimeOffset.UtcNow;        // Save finish time
            scan.Error = ex.Message;                        // Store error message
            await _db.SaveChangesAsync(ct);                 // Persist failure status
        }
    }
}