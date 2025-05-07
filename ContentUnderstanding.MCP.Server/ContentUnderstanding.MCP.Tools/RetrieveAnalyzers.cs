using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContentUnderstanding.MCP.Tools;

public class RetrieveAnalyzers
{
    private readonly ContentUnderstandingClient _contentUnderstandingClient;

    public RetrieveAnalyzers(ContentUnderstandingClient contentUnderstandingClient)
    {
        _contentUnderstandingClient = contentUnderstandingClient;
    }

    [KernelFunction, Description("Retreives a list of analyzers for Content Understanding.")]
    public async Task<IEnumerable<string>> GetAnalyzers()
    {
        var analyzers = await _contentUnderstandingClient.GetAnalyzers();
        return analyzers.Value.Select(a => $"Analyzer Name: {a.AnalyzerId} with description: {a.Description}");
    }
}
