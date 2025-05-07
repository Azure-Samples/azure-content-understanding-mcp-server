using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContentUnderstanding.MCP.Tools
{
    public class AnalyzeDocument
    {
        private readonly ContentUnderstandingClient _contentUnderstandingClient;

        public AnalyzeDocument(ContentUnderstandingClient contentUnderstandingClient)
        {
            _contentUnderstandingClient = contentUnderstandingClient;
        }

        [KernelFunction, Description("Submits a document to Azure Content Understanding to extract content from.")]
        public async Task<DocumentAnalysisResponse> Analyze(
            [Description("Single analyzer id to use to analyze the document")] string analyzerId,
            [Description("Full file path of the document.")]string fullFilePath
        ) {
            AnalysisResponse analysisResponse = await _contentUnderstandingClient.AnalyzeBinaryAsnc("test-extraction", fullFilePath);
            DocumentAnalysisResponse analyzerResponse = null;

            if (String.IsNullOrEmpty(analysisResponse.Id))
            {
                return null;
            }

            do
            {
                await Task.Delay(1000);
                analyzerResponse = await _contentUnderstandingClient.GetAnalysisContent(analysisResponse.Id, analyzerId);
            } while (analyzerResponse.Status == "Running");

            return analyzerResponse;
        }
    }
}
