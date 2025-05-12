using Azure;
using Azure.Core;
using ContentUnderstanding.MCP.Tools.models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ContentUnderstanding.MCP.Tools.Clients
{
    /// <summary>
    /// Client for interacting with Azure Content Understanding service.
    /// Provides methods to create, retrieve, and use document analyzers.
    /// </summary>
    public class ContentUnderstandingClient
    {
        private readonly HttpClient _httpClient;
        private readonly AzureKeyCredential _keyCredential;
        private readonly string _apiVersion;

        /// <summary>
        /// Initializes a new instance of the ContentUnderstandingClient.
        /// </summary>
        /// <param name="endpoint">The base URL of the Content Understanding API.</param>
        /// <param name="apiKey">API key for authentication with the service.</param>
        /// <param name="apiVersion">Version of the Content Understanding API to use.</param>
        /// <param name="httpClient">Optional HTTP client to use for requests. If null, a new client will be created.</param>
        /// <param name="credentials">Optional token credentials for authentication. Not currently used.</param>
        public ContentUnderstandingClient(
            string endpoint,
            string apiKey,
            string apiVersion,
            HttpClient? httpClient = null,
            TokenCredential? credentials = null
        ) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(endpoint) };
            _keyCredential = new AzureKeyCredential(apiKey);
            _apiVersion = apiVersion;
        }

        /// <summary>
        /// Creates or updates an analyzer with the specified ID and schema.
        /// </summary>
        /// <param name="analyzerId">Unique identifier for the analyzer to create or update.</param>
        /// <param name="schema">JSON schema that defines the analyzer's behavior and structure.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        public async Task CreateAnalyzerAsync(string analyzerId, string schema)
        {
            var createAnalyzerEndpoint = $"/contentunderstanding/analyzers/{analyzerId}?api-version={_apiVersion}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            StringContent content = new(schema, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(createAnalyzerEndpoint, content);

            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Retrieves all available analyzers from the Content Understanding service.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the response with information about available analyzers.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="JsonException">Thrown when the response cannot be deserialized.</exception>
        public async Task<AnalyzerResponse> GetAnalyzers()
        {
            var getAnalyzersEndpoint = $"/contentunderstanding/analyzers?api-version={_apiVersion}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            HttpResponseMessage response = await _httpClient.GetAsync(getAnalyzersEndpoint);

            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AnalyzerResponse>(jsonString, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Submits a document at the specified URL for analysis using the specified analyzer.
        /// </summary>
        /// <param name="analyzerId">ID of the analyzer to use for document analysis.</param>
        /// <param name="fileUrl">URL of the file to analyze (typically a blob storage URL).</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains initial analysis response with status and ID.
        /// </returns>
        /// <remarks>
        /// This method initiates analysis but does not wait for it to complete.
        /// Use GetAnalysisContent with the returned ID to retrieve results.
        /// </remarks>
        public async Task<AnalysisResponse> AnalyzeAsnc(string analyzerId, string fileUrl)
        {
            var analyzeFileEndpoint = $"/contentunderstanding/analyzers/{analyzerId}:analyze?api-version={_apiVersion}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync<AnalyzeDocumentRequest>(analyzeFileEndpoint, new(fileUrl));

            return await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        }

        /// <summary>
        /// Retrieves the results of a previously initiated document analysis.
        /// </summary>
        /// <param name="analysisId">ID of the analysis to retrieve results for.</param>
        /// <param name="analyzerId">ID of the analyzer that was used.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the document analysis response with status and results.
        /// </returns>
        /// <remarks>
        /// This method should be called after initiating analysis with AnalyzeAsnc.
        /// Check the Status property of the response to determine if analysis is complete.
        /// </remarks>
        public async Task<DocumentAnalysisResponse> GetAnalysisContent(string analysisId, string analyzerId)
        {
            var analyzeUrlEndpoint = $"contentunderstanding/analyzers/{analyzerId}/results/{analysisId}?api-version={_apiVersion}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            HttpResponseMessage response = await _httpClient.GetAsync(analyzeUrlEndpoint);
            return await response.Content.ReadFromJsonAsync<DocumentAnalysisResponse>();
        }
    }
}
