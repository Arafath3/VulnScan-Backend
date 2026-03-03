namespace VulnScanner.Api.Models;                   // Namespace grouping for model types

public sealed class Target                          // A "Target" is something you will scan (a URL)
{
    public Guid Id { get; set; }                    // Primary key column in SQL (Targets.Id)
    public string BaseUrl { get; set; } = "";       // The URL to scan (stored in SQL)
    public DateTimeOffset CreatedAt { get; set; }   // Timestamp column
        = DateTimeOffset.UtcNow;                    // Default value when a row is created
}