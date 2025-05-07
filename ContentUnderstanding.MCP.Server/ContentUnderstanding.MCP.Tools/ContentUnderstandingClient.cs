using Azure;
using Azure.Core;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ContentUnderstanding.MCP.Tools
{
    public class ContentUnderstandingClient
    {
        private readonly HttpClient _httpClient;
        private readonly AzureKeyCredential _keyCredential;
        private readonly string _apiVersion;

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

        public async Task CreateAnalyzerAsync(string analyzerId, string schema)
        {
            var createAnalyzerEndpoint = $"/contentunderstanding/analyzers/{analyzerId}?api-version={_apiVersion}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            StringContent content = new(schema, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(createAnalyzerEndpoint, content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<AnalyzerResponse> GetAnalyzers()
        {
            var getAnalyzersEndpoint = $"/contentunderstanding/analyzers?api-version={_apiVersion}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            HttpResponseMessage response = await _httpClient.GetAsync(getAnalyzersEndpoint);

            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AnalyzerResponse>(jsonString, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        public Task DeleteAnalyzerAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<AnalysisResponse> AnalyzeBinaryAsnc(string analyzerId, string filePath)
        {
            var analyzeFileEndpoint = $"/contentunderstanding/analyzers/{analyzerId}:analyze?_overload=analyzeBinary&api-version={_apiVersion}";

            // Read all bytes from the file and convert to base64
            string base64Content = Convert.ToBase64String(File.ReadAllBytes(filePath));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);

            // Send the base64 encoded content
            var content = new StringContent(base64Content, Encoding.UTF8, "application/base64");
            HttpResponseMessage response = await _httpClient.PostAsync(analyzeFileEndpoint, content);

            return await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        }

        public async Task<DocumentAnalysisResponse> GetAnalysisContent(string analysisId, string analyzerId)
        {

            var analyzeUrlEndpoint = $"contentunderstanding/analyzers/{analyzerId}/results/{analysisId}?api-version={_apiVersion}";
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _keyCredential.Key);
            
            HttpResponseMessage response = await _httpClient.GetAsync(analyzeUrlEndpoint);
            return await response.Content.ReadFromJsonAsync<DocumentAnalysisResponse>();
        }
    }
}
