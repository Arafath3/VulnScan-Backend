namespace VulnScanner.Api.Models;                   // Namespace

public sealed class Finding                          // A Finding is one result produced by a scan
{
    public Guid Id { get; set; }                    // Primary key column in SQL (Findings.Id)
    public Guid ScanRunId { get; set; }             // Foreign key to ScanRuns.Id
    public string Severity { get; set; } = "Low";   // Severity text (Low/Medium/High/Critical)
    public string Title { get; set; } = "";         // Short name for the issue
    public string EvidenceRef { get; set; } = "";   // Reference string to evidence (file path later)
}