using Azure.Storage.Blobs;
using ContentUnderstanding.MCP.Tools.Clients;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContentUnderstanding.MCP.Tools
{
    /// <summary>
    /// Provides functionality to analyze documents using Azure Content Understanding service.
    /// Handles document upload, analysis, and result retrieval.
    /// </summary>
    public class AnalyzeDocument
    {
        private readonly ContentUnderstandingClient _contentUnderstandingClient;
        private readonly BlobContainerClient _blobContainerClient;

        /// <summary>
        /// Initializes a new instance of the AnalyzeDocument class.
        /// </summary>
        /// <param name="contentUnderstandingClient">Client for the Content Understanding API.</param>
        /// <param name="blobContainerClient">Client for Azure Blob Storage operations.</param>
        /// <param name="logger">Optional logger for diagnostic information.</param>
        public AnalyzeDocument(
            ContentUnderstandingClient contentUnderstandingClient,
            BlobContainerClient blobContainerClient)
        {
            _contentUnderstandingClient = contentUnderstandingClient ?? throw new ArgumentNullException(nameof(contentUnderstandingClient));
            _blobContainerClient = blobContainerClient ?? throw new ArgumentNullException(nameof(blobContainerClient));
        }

        /// <summary>
        /// Analyzes a document using the specified analyzer.
        /// The process uploads the document to blob storage, submits it for analysis,
        /// polls for completion, and cleans up the blob when done.
        /// </summary>
        /// <param name="analyzerId">The ID of the analyzer to use.</param>
        /// <param name="fullFilePath">The full path to the file to analyze.</param>
        /// <param name="timeoutSeconds">Optional timeout in seconds (default: 300 seconds/5 minutes).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The analysis response containing results.</returns>
        /// <exception cref="ArgumentException">Thrown if arguments are invalid.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the specified file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the analysis couldn't be started properly.</exception>
        /// <exception cref="TimeoutException">Thrown if the analysis doesn't complete within the timeout period.</exception>
        [KernelFunction, Description("Submits a document to Azure Content Understanding to extract content from.")]
        public async Task<DocumentAnalysisResponse> Analyze(
            [Description("Single analyzer id to use to analyze the document")] string analyzerId,
            [Description("Full file path of the document.")] string fullFilePath
        ) {
            var runningStatus = "Running";
            var timeoutSeconds = 300;

            // Parameter validation
            if (string.IsNullOrEmpty(analyzerId))
            {
                throw new ArgumentException("Analyzer ID must be provided", nameof(analyzerId));
            }

            if (string.IsNullOrEmpty(fullFilePath))
            {
                throw new ArgumentException("File path must be provided", nameof(fullFilePath));
            }

            // Verify file exists
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException("The document to analyze could not be found.", fullFilePath);
            }

            // Create unique blob name using a combination of file name and timestamp
            string fileName = Path.GetFileName(fullFilePath);
            string blobName = $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.UtcNow.Ticks}{Path.GetExtension(fileName)}";

            // Get blob client
            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

            try
            {
                // Upload blob
                await using (FileStream fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    await blobClient.UploadAsync(fileStream, overwrite: true).ConfigureAwait(false);
                }

                // Submit for analysis
                AnalysisResponse analysisResponse = await _contentUnderstandingClient.AnalyzeAsnc(analyzerId, blobClient.Uri.ToString()).ConfigureAwait(false);

                if (string.IsNullOrEmpty(analysisResponse.Id))
                {
                    throw new InvalidOperationException("Failed to start analysis. No analysis ID was returned.");
                }

                // Create a timeout cancellation source
                using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token);

                // Poll for results with exponential backoff
                DocumentAnalysisResponse analyzerResponse;
                int attemptCount = 0;
                int currentDelay = 1000;
                int maxAttempts = (int)(timeoutSeconds * 1000 / currentDelay); // Maximum number of polling attempts

                do
                {
                    try
                    {
                        await Task.Delay(currentDelay, linkedSource.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) when (timeoutSource.Token.IsCancellationRequested)
                    {
                        throw new TimeoutException($"Analysis did not complete within the specified timeout of {timeoutSeconds} seconds.");
                    }

                    attemptCount++;

                    analyzerResponse = await _contentUnderstandingClient
                        .GetAnalysisContent(analysisResponse.Id, analyzerId)
                        .ConfigureAwait(false);
                } while (analyzerResponse.Status == runningStatus && attemptCount < maxAttempts);

                // Check if analysis is still running after max attempts
                if (analyzerResponse.Status == runningStatus)
                {
                    throw new TimeoutException($"Analysis did not complete within the specified timeout of {timeoutSeconds} seconds.");
                }

                return analyzerResponse;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unexpected error analyzing document", ex);
            }
            finally
            {
                await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }
    }
}
