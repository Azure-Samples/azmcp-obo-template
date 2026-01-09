# Connect from C# McpClient

Connect to Azure MCP Server from a C# console app.

## Prerequisites

- Azure MCP Server deployed and running (see [main README](../README.md))
- .NET 10 SDK

## Setup

1. **Update `appsettings.json` with your MCP server URL**

Edit `appsettings.json` and set the `McpServer:Url` to your Container App URL:

```json
{
  "McpServer": {
    "Url": "https://<your_mcp_server_endpoint>"
  },
  "EntraClientClientId": "<ENTRA_APP_CLIENT_CLIENT_ID>"
}
```

You can retrieve these values using:
```bash
# Url
azd env get-value CONTAINER_APP_URL
# Entra Client Client Id
azd env get-value ENTRA_APP_CLIENT_CLIENT_ID
```

2. **Build and run**

```bash
dotnet build && dotnet run
```

The client will:
- Fetch OAuth Protected Resource Metadata from the server
- Authenticate using interactive browser login
- Connect to the MCP server and list available tools
- Optional: Call `storage_account_get` tool to list storage accounts in a subscription. Make sure to toggle the flag and fill your subscription id before calling the tool.
