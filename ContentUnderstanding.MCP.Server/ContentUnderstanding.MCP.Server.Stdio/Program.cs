using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ContentUnderstanding.MCP.Server.Stdio.Extensions;
using ContentUnderstanding.MCP.Tools;
using ContentUnderstanding.MCP.Tools.Clients;
using ContentUnderstanding.MCP.Server.Stdio.Utils;
using Azure.Storage.Blobs;
using Azure.Identity;

// Create a new generic host builder for the application
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Configure application settings from appsettings.json, user secrets, and environment variables
builder.Configuration
  .AddJsonFile("appsettings.json", optional: true)
  .AddUserSecrets<Program>()
  .AddEnvironmentVariables();

// Validate required configuration values are present
Guard.ThrowIfNullOrEmpty(builder.Configuration["STORAGE_CONTAINER_URL"], "STORAGE_CONTAINER_URL");
Guard.ThrowIfNullOrEmpty(builder.Configuration["ENDPOINT"], "ENDPOINT");
Guard.ThrowIfNullOrEmpty(builder.Configuration["API_KEY"], "API_KEY");
Guard.ThrowIfNullOrEmpty(builder.Configuration["API_VERSION"], "API_VERSION");

// Configure logging: read settings from configuration, log to file, set minimum level to Debug
builder.Services.AddLogging(configure =>
  configure
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddFile(o => o.RootPath = AppContext.BaseDirectory)
    .SetMinimumLevel(LogLevel.Debug)
);

// Register the Semantic Kernel service
builder.Services.AddKernel();

// Register plugin services for document analysis and analyzer retrieval
builder.Services.AddSingleton(sp => KernelPluginFactory.CreateFromType<RetrieveAnalyzers>(serviceProvider: sp));
builder.Services.AddSingleton(sp => KernelPluginFactory.CreateFromType<AnalyzeDocument>(serviceProvider: sp));

// Register the ContentUnderstandingClient and BlobContainerClient as singletons
builder.Services.AddSingleton(new BlobContainerClient(
    new Uri(builder.Configuration["STORAGE_CONTAINER_URL"]),
    new DefaultAzureCredential())
);
builder.Services.AddSingleton(new ContentUnderstandingClient(
    builder.Configuration["ENDPOINT"],
    builder.Configuration["API_KEY"],
    builder.Configuration["API_VERSION"])
);

// Build the service provider and retrieve the Semantic Kernel instance
var kernel = builder.Services.BuildServiceProvider().GetRequiredService<Kernel>();

// Register and configure the MCP server with stdio transport and tool plugins
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools(kernel.Plugins);

// Build the application host
var app = builder.Build();

// Get a logger instance for the Program class
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Log startup message
logger.LogInformation("Starting Content Understanding MCP Server");

// Run the application asynchronously
await app.RunAsync();

// Log shutdown message
logger.LogInformation("Shutting down Content Understanding MCP Server");
