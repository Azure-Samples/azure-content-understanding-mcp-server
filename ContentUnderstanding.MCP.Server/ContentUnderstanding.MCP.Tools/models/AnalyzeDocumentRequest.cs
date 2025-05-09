namespace ContentUnderstanding.MCP.Tools.models
{
    /// <summary>
    /// Represents a request to analyze a document using Azure Content Understanding service.
    /// </summary>
    /// <param name="Url">
    /// The URL pointing to the document to be analyzed. This should be a valid URL to a document 
    /// accessible by the Content Understanding service, typically a blob storage URL.
    /// </param>
    /// <remarks>
    /// This record is used when submitting document analysis requests to the Content Understanding API.
    /// It encapsulates the minimal information needed to locate and process the document.
    /// </remarks>
    public record AnalyzeDocumentRequest(string Url);
}
