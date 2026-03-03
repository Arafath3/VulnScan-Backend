using Microsoft.EntityFrameworkCore;                // EF Core base types
using VulnScanner.Api.Models;                       // Import your model classes

namespace VulnScanner.Api.Data;                     // Namespace for data access

public sealed class AppDbContext : DbContext         // DbContext represents a session with the database
{
    public AppDbContext(DbContextOptions<AppDbContext> options) // Options contain connection/provider config
        : base(options)                              // Pass options to EF Core base class
    {
    }

    public DbSet<Target> Targets => Set<Target>();   // Targets table accessor
    public DbSet<ScanRun> ScanRuns => Set<ScanRun>(); // ScanRuns table accessor
    public DbSet<Finding> Findings => Set<Finding>(); // Findings table accessor
}