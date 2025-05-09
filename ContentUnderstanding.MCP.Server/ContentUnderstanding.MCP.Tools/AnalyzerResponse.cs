namespace ContentUnderstanding.MCP.Tools
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public interface IField
    {
        string Type { get; }
        string Method { get; }
        string Description { get; }
    }

    public record SimpleField(
        string Type,
        string Method,
        string Description
    ) : IField;

    public record EnumField(
        string Type,
        string Method,
        string Description,
        IReadOnlyList<string> Enum
    ) : IField;

    public record ArrayField(
        string Type,
        string Method,
        string Description,
        IField Items
    ) : IField;

    public record ObjectField(
        string Type,
        string Method,
        string Description,
        IReadOnlyDictionary<string, IField> Properties
    ) : IField;

    public interface IFieldSchema
    {
        IReadOnlyDictionary<string, IField> Fields { get; }
    }

    public record FieldSchema(
        IReadOnlyDictionary<string, IField> Fields
    ) : IFieldSchema;

    public record Config(
        bool ReturnDetails,
        IReadOnlyList<string> Locales
    );

    public record DocumentSchema(
        string Description,
        string Scenario,
        Config Config,
        IFieldSchema FieldSchema
    );

    public record AnalyzerResponse(
        IReadOnlyList<Analyzer> Value
    );

    public record Analyzer(
        string AnalyzerId,
        string Description,
        AnalyzerConfig Config,
        IReadOnlyList<string> Warnings,
        string Status,
        string Scenario
    );

    public record AnalyzerConfig(
        bool ReturnDetails,
        bool EnableOcr,
        bool EnableLayout,
        bool EnableBarcode,
        bool EnableFormula
    );


    //NEW

    public record DocumentAnalysisResponse
    {
        public string Id { get; init; } = null!;
        public string Status { get; init; } = null!;
        public DocumentAnalysisResult Result { get; init; } = null!;
    }

    public record DocumentAnalysisResult
    {
        public string AnalyzerId { get; init; } = null!;
        public string ApiVersion { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public List<string> Warnings { get; init; } = new();
        public List<DocumentContent> Contents { get; init; } = new();
    }

    public record DocumentContent
    {
        public string Markdown { get; init; } = null!;
        public Dictionary<string, ContentField> Fields { get; init; } = new();
        public string Kind { get; init; } = null!;
        public int StartPageNumber { get; init; }
        public int EndPageNumber { get; init; }
        public List<object> Pages { get; init; } = new();
    }

    public record ContentField
    {
        public string Type { get; init; } = null!;
        public string? ValueString { get; init; }
    }

    public record AnalysisResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("status")]
        public string Status { get; init; }

        [JsonPropertyName("result")]
        public AnalysisResult Result { get; init; }
    }

    public record AnalysisResult
    {
        [JsonPropertyName("analyzerId")]
        public string AnalyzerId { get; init; }

        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; init; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; init; }

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; init; }

        [JsonPropertyName("contents")]
        public List<object> Contents { get; init; }
    }
}
