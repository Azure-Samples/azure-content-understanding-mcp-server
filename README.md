# Azure Content Understanding MCP Server
This project provides a Model Context Protocol (MCP) server for integrating Azure Content Understanding capabilities with AI systems. The server allows content analysis tools to be exposed through a standard MCP interface.

## Project Structure
- ContentUnderstanding.MCP.Server.Stdio: Core MCP server implementation using standard I/O for communication
- ContentUnderstanding.MCP.Tools: Library containing tools for content analysis and Azure - Content Understanding API integration

## Features
- Document Analysis: Analyze various file formats using Azure Content Understanding service
- Multiple Analyzer Support: Retrieve and use different analyzers based on content type or your data requirements.

## Getting Started
Prerequisites
- .NET 9.0 SDK
- Azure subscription with Content Understanding service
- Azure Blob Storage account

## Tools and Components
### Content Understanding Client
The ContentUnderstandingClient provides methods for interacting with Azure Content Understanding:

- Create and update analyzers
- List available analyzers
- Submit documents for analysis
- Retrieve analysis results

### Document Analysis
The AnalyzeDocument class handles the end-to-end process:

1. Upload the document to Azure Blob Storage
2. Submit for analysis via Content Understanding API
3. Poll for analysis completion
4. Return structured results
5. Clean up temporary blob storage

## Sample MCP Configuration
```json
"contentUnderstanding": {
    "command": "dotnet",
    "args": [
        "run",
        "--project",
        "FULL_PATH_TO_PROJECT_FOLDER",
        "--no-build"
    ],
    "env": {
        "ENDPOINT": "CONTENT_UMDERSTANDING_ENDPOINT",
        "API_KEY": "CONTENT_UMDERSTANDING_API_KEY",
        "API_VERSION": "CONTENT_UMDERSTANDING_VERSION",
        "STORAGE_CONTAINER_URL": "STORAGE_CONTAINER_URL",
        "ALLOWED_DIRECTORIES": "SEMICOLON_SEPERATED_LIST_OF_FOLDERS"
    }
}
```