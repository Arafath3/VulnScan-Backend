using Microsoft.AspNetCore.Mvc;        // API controller types
using Microsoft.EntityFrameworkCore;   // EF Core async query methods
using VulnScanner.Api.Data;            // AppDbContext
using VulnScanner.Api.Models;          // ScanRun, ScanStatus, Finding
using VulnScanner.Api.Services;        // Queue types

namespace VulnScanner.Api.Controllers; // Controllers namespace

[ApiController]                        // Enables API behaviors and binding
[Route("api/scans")]                   // Base route: /api/scans
public sealed class ScansController : ControllerBase // Controller base provides Ok(), NotFound(), etc.
{
    private readonly AppDbContext _db;              // DB context for CRUD
    private readonly IBackgroundTaskQueue _queue;   // Queue for scan jobs

    public ScansController(AppDbContext db, IBackgroundTaskQueue queue) // DI injects both
    {
        _db = db;                                   // Store db
        _queue = queue;                             // Store queue
    }

    public sealed class StartScanRequest            // DTO for POST body
    {
        public Guid TargetId { get; set; }          // Which target to scan
    }

    [HttpPost]                                      // POST /api/scans
    public async Task<ActionResult<ScanRun>> Start( // Starts a scan run
        [FromBody] StartScanRequest req,            // Read JSON body
        CancellationToken ct                        // Cancellation token
    )
    {
        var target = await _db.Targets              // Validate target exists
            .FirstOrDefaultAsync(x => x.Id == req.TargetId, ct);

        if (target is null)                         // If invalid target id
            return NotFound("Target not found");    // Return 404

        var scan = new ScanRun                      // Create ScanRun row
        {
            Id = Guid.NewGuid(),                    // New primary key
            TargetId = target.Id,                   // Foreign key to target
            Status = ScanStatus.Queued,             // Initial state
            CreatedAt = DateTimeOffset.UtcNow       // Creation time
        };

        _db.ScanRuns.Add(scan);                     // Stage insert in DB
        await _db.SaveChangesAsync(ct);             // Persist ScanRun

        await _queue.EnqueueAsync(                  // Enqueue the scan job for the worker
            new ScanRequest(scan.Id),               // Message includes scan id
            ct                                      // Cancellation token
        );

        return Ok(scan);                            // Return created ScanRun
    }

    [HttpGet("{id:guid}")]                          // GET /api/scans/{id}
    public async Task<ActionResult<ScanRun>> Get(   // Read scan status
        Guid id,                                    // ScanRun id from route
        CancellationToken ct                        // Cancellation token
    )
    {
        var scan = await _db.ScanRuns               // Query ScanRuns
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return scan is null ? NotFound() : Ok(scan); // Return 404 or 200
    }

    [HttpGet("{id:guid}/findings")]                 // GET /api/scans/{id}/findings
    public async Task<ActionResult<List<Finding>>> Findings( // Read findings for scan
        Guid id,                                    // ScanRun id
        CancellationToken ct                        // Cancellation token
    )
    {
        var items = await _db.Findings              // Query Findings
            .Where(x => x.ScanRunId == id)          // Filter by scan
            .ToListAsync(ct);                       // Execute query

        return Ok(items);                           // Return list
    }
}