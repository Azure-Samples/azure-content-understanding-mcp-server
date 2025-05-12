using ContentUnderstanding.MCP.Tools.Clients;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContentUnderstanding.MCP.Tools
{
    /// <summary>
    /// Provides functionality to retrieve and format available analyzers from the Azure Content Understanding service.
    /// This class serves as a plugin for the Semantic Kernel framework.
    /// </summary>
    public class RetrieveAnalyzers
    {
        private readonly ContentUnderstandingClient _contentUnderstandingClient;

        /// <summary>
        /// Initializes a new instance of the RetrieveAnalyzers class.
        /// </summary>
        /// <param name="contentUnderstandingClient">The client used to communicate with the Content Understanding service.</param>
        public RetrieveAnalyzers(ContentUnderstandingClient contentUnderstandingClient)
        {
            _contentUnderstandingClient = contentUnderstandingClient;
        }

        /// <summary>
        /// Retrieves a list of available analyzers from the Content Understanding service and formats them as human-readable strings.
        /// This method is exposed as a Semantic Kernel function that can be called by AI systems or other components.
        /// </summary>
        /// <returns>
        /// A collection of formatted strings, each describing an available analyzer with its ID and description.
        /// Format: "Analyzer Name: {analyzerId} with description: {description}"
        /// </returns>
        /// <remarks>
        /// Each string in the returned collection represents one analyzer and includes both its identifier
        /// and description to provide context about the analyzer's purpose and capabilities.
        /// </remarks>
        [KernelFunction, Description("Retreives a list of analyzers for Content Understanding.")]
        public async Task<IEnumerable<string>> GetAnalyzers()
        {
            var analyzers = await _contentUnderstandingClient.GetAnalyzers();
            return analyzers.Value.Select(a => $"Analyzer Name: {a.AnalyzerId} with description: {a.Description}");
        }
    }
}
