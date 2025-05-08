using Azure.Storage.Blobs;
using ContentUnderstanding.MCP.Tools.Clients;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContentUnderstanding.MCP.Tools
{
    public class AnalyzeDocument
    {
        private readonly ContentUnderstandingClient _contentUnderstandingClient;
        private readonly BlobContainerClient _blobContainerClient;

        public AnalyzeDocument(ContentUnderstandingClient contentUnderstandingClient, BlobContainerClient blobContainerClient)
        {
            _contentUnderstandingClient = contentUnderstandingClient;
            _blobContainerClient = blobContainerClient;
        }

        [KernelFunction, Description("Submits a document to Azure Content Understanding to extract content from.")]
        public async Task<DocumentAnalysisResponse> Analyze(
            [Description("Single analyzer id to use to analyze the document")] string analyzerId,
            [Description("Full file path of the document.")] string fullFilePath
        )
        {
            // Create unique blob name using a combination of file name and timestamp
            string fileName = Path.GetFileName(fullFilePath);
            string blobName = $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.UtcNow.Ticks}{Path.GetExtension(fileName)}";

            // Ensure container exists

            // Upload blob
            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);
            await using (FileStream fileStream = new FileStream(fullFilePath, FileMode.Open))
            {
                await blobClient.UploadAsync(fileStream, overwrite: true);
            }

            // Continue with regular processing
            AnalysisResponse analysisResponse = await _contentUnderstandingClient.AnalyzeAsnc(analyzerId, blobClient.Uri.ToString());
            DocumentAnalysisResponse analyzerResponse = null;

            if (String.IsNullOrEmpty(analysisResponse.Id))
            {
                // Delete blob if analysis fails to start
                await blobClient.DeleteIfExistsAsync();
                return null;
            }

            do
            {
                await Task.Delay(1000);
                analyzerResponse = await _contentUnderstandingClient.GetAnalysisContent(analysisResponse.Id, analyzerId);
            } while (analyzerResponse.Status == "Running");

            // Delete blob after analysis is complete
            await blobClient.DeleteIfExistsAsync();

            return analyzerResponse;
        }
    }
}
