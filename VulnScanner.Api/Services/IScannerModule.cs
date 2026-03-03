using VulnScanner.Api.Models;         // Needed for Target and Finding models

namespace VulnScanner.Api.Services;   // Services namespace

public interface IScannerModule       // “Plugin” contract for scanner modules
{
    string Name { get; }              // Human label for the module

    Task<IReadOnlyList<Finding>> RunAsync( // Runs a module and returns findings
        Target target,                     // What to scan (URL)
        Guid scanRunId,                    // Which ScanRun these findings belong to
        CancellationToken ct               // Cancellation token
    );
}