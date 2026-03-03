using Microsoft.AspNetCore.Mvc;                         // Controller types and attributes
using Microsoft.EntityFrameworkCore;                    // Async query helpers
using VulnScanner.Api.Data;                             // AppDbContext
using VulnScanner.Api.Models;                           // Target model

namespace VulnScanner.Api.Controllers;                  // Namespace for controllers

[ApiController]                                         // Enables API behaviors (binding, validation)
[Route("api/targets")]                                  // Base route: /api/targets
public sealed class TargetsController : ControllerBase   // Base class with Ok(), NotFound(), etc.
{
    private readonly AppDbContext _db;                   // Database context field

    public TargetsController(AppDbContext db)            // DI injects AppDbContext per request
    {
        _db = db;                                       // Store it in the field
    }

    public sealed class CreateTargetRequest              // DTO for incoming JSON body
    {
        public string BaseUrl { get; set; } = "";        // Client sends { "baseUrl": "https://..." }
    }

    [HttpPost]                                           // POST /api/targets
    public async Task<ActionResult<Target>> Create([FromBody] CreateTargetRequest req, CancellationToken ct)
    {
        var t = new Target                               // Create a Target entity
        {
            Id = Guid.NewGuid(),                         // Assign a new Guid
            BaseUrl = req.BaseUrl.Trim(),                // Save trimmed URL
            CreatedAt = DateTimeOffset.UtcNow            // Set created time
        };

        _db.Targets.Add(t);                              // Stage insert into Targets table
        await _db.SaveChangesAsync(ct);                  // Execute INSERT in SQL Server

        return Ok(t);                                    // Return created target as JSON
    }

    [HttpGet]                                            // GET /api/targets
    public async Task<ActionResult<List<Target>>> List(CancellationToken ct)
    {
        var items = await _db.Targets                    // Start query on Targets table
            .OrderByDescending(x => x.CreatedAt)         // Sort newest first
            .ToListAsync(ct);                            // Execute query and return list

        return Ok(items);                                // Return list as JSON
    }
}