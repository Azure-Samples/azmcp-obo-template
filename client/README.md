# Connect from C# McpClient

Connect to Azure MCP Server from a C# console app.

The client will:
- Fetch OAuth Protected Resource Metadata from the server
- Authenticate using interactive browser login
- Connect to the MCP server and list available tools
- **Optional**: Call `storage_account_get` tool to list storage accounts in a subscription.


## Prerequisites

- Azure MCP Server deployed and running (see [main README](../README.md))
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Setup

1. **Update `appsettings.json` with your MCP server URL**

    Edit `appsettings.json` and set the `McpServer:Url` to your Container App URL:
    ```json
    // If MCP server was deployed using repo's infrastructure,
    // the commented azd commands can be used to obtain the value.
    {
      "McpServer": {

        // `azd env get-value CONTAINER_APP_URL`
        "Url": "https://<your_mcp_server_endpoint>"
      },

      // `azd env get-value ENTRA_APP_CLIENT_CLIENT_ID`
      "EntraClientClientId": "<entraapp_mcpclient_clientid>",

      // `azd env get-value AZURE_SUBSCRIPTION_ID`
      "SubscriptionId": "<your_subscription_id>"
    }
    ```

    > **Tip:** You can also create an `appsettings.Development.json` file with your local overrides.
    > Set the `DOTNET_ENVIRONMENT` environment variable to `Development` to load it on top of `appsettings.json`:
    >
    > ```bash
    > # PowerShell
    > $env:DOTNET_ENVIRONMENT="Development"
    > ```

2. **Build and run**
    ```bash
    dotnet run
    ```

    To perform an additional tool call to list storage accounts, run:

    ```bash
    dotnet run -- --list-accounts true
    ```

    > **Note:** All `appsettings.json` values can also be overridden via command-line arguments
    > (e.g., `--McpServer:Url https://localhost:7071`).

## Troubleshooting

If you encounter authentication or consent errors (e.g., `MsalUiRequiredException`), see the
[Troubleshooting guide](../Troubleshooting.md).
