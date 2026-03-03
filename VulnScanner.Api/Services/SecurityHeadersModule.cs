using VulnScanner.Api.Models;           // Import Target and Finding

namespace VulnScanner.Api.Services;     // Services namespace

public sealed class SecurityHeadersModule : IScannerModule // One safe module: checks HTTP headers
{
    private readonly IHttpClientFactory _http;              // Correct way to create HttpClient in .NET
    public string Name => "SecurityHeaders";                // Module name

    public SecurityHeadersModule(IHttpClientFactory http)   // Dependency injection provides the factory
        => _http = http;                                   // Store it

    public async Task<IReadOnlyList<Finding>> RunAsync(     // Module execution entry point
        Target target,                                      // Target from DB
        Guid scanRunId,                                     // ScanRun id for linking findings
        CancellationToken ct                                // Cancellation token
    )
    {
        var findings = new List<Finding>();                 // Collected results

        if (!Uri.TryCreate(target.BaseUrl, UriKind.Absolute, out var uri)) // Validate BaseUrl
            return findings;                                                // Invalid URL, return empty list

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) // Restrict schemes
            return findings;                                                        // Not http/https, stop

        var client = _http.CreateClient();                 // Create an HttpClient instance

        using var req = new HttpRequestMessage(HttpMethod.Get, uri); // Build a GET request
        req.Headers.UserAgent.ParseAdd("VulnScanner/1.0");           // Add a simple User-Agent header

        using var res = await client.SendAsync(req, ct);            // Send request and get response

        bool Has(string h)                                          // Helper to test header existence
            => res.Headers.Contains(h) || res.Content.Headers.Contains(h); // Check both header sets

        void Add(string severity, string title, string evidence)    // Helper to create a Finding row
        {
            findings.Add(new Finding
            {
                Id = Guid.NewGuid(),                                // New Finding primary key
                ScanRunId = scanRunId,                              // Link Finding to ScanRun
                Severity = severity,                                // Severity classification
                Title = title,                                      // Finding title
                EvidenceRef = evidence                              // Simple evidence text for now
            });
        }

        if (!Has("Strict-Transport-Security"))                      // Missing HSTS
            Add("Medium", "Missing HSTS header", "Strict-Transport-Security not present");

        if (!Has("Content-Security-Policy"))                        // Missing CSP
            Add("Medium", "Missing CSP header", "Content-Security-Policy not present");

        if (!Has("X-Content-Type-Options"))                         // Missing X-Content-Type-Options
            Add("Low", "Missing X-Content-Type-Options header", "X-Content-Type-Options not present");

        if (!Has("X-Frame-Options"))                                // Missing X-Frame-Options
            Add("Low", "Missing X-Frame-Options header", "X-Frame-Options not present");

        if (!Has("Referrer-Policy"))                                // Missing Referrer-Policy
            Add("Low", "Missing Referrer-Policy header", "Referrer-Policy not present");

        return findings;                                            // Return all findings to caller
    }
}