// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;

namespace ContentUnderstanding.MCP.Server.Stdio.Extensions;

/// <summary>
/// Extension methods for <see cref="IMcpServerBuilder"/>.
/// </summary>
public static class McpServerBuilderExtensions
{
  /// <summary>
  /// Adds all functions of the kernel plugins as tools to the server.
  /// </summary>
  /// <param name="builder">The builder instance.</param>
  /// <param name="plugins">The kernel plugins to add as tools to the server.</param>
  /// <returns>The builder instance.</returns>
  public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, KernelPluginCollection plugins)
  {
    foreach (var plugin in plugins)
    {
      foreach (var function in plugin)
      {
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddSingleton(services => McpServerTool.Create(function.AsAIFunction()));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
      }
    }

    return builder;
  }
}