using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Net.Http.Headers;
using ContentUnderstanding.MCP.Server.Stdio.Extensions;
using ContentUnderstanding.MCP.Tools;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Configuration
  .AddJsonFile("appsettings.json", optional: true)
  .AddUserSecrets<Program>()
  .AddEnvironmentVariables();

builder.Services.AddLogging(configure =>
  configure
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddFile(o => o.RootPath = AppContext.BaseDirectory)
    .SetMinimumLevel(LogLevel.Debug)
);

builder.Services.AddKernel();

builder.Services.AddSingleton(sp => KernelPluginFactory.CreateFromType<DummyTool>(serviceProvider: sp));

var apiKey = builder.Configuration["Ynab:ApiKey"];

builder.Services.AddHttpClient();

var kernel = builder.Services.BuildServiceProvider().GetRequiredService<Kernel>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools(kernel.Plugins);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Starting Content Understanding MCP Server");
await app.RunAsync();
logger.LogInformation("Shutting down Content Understanding MCP Server");
