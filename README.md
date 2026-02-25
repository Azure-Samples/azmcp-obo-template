# Azure MCP Server — Remote Hosting with On-Behalf-Of (OBO) Flow

Deploy the [Azure MCP Server 2.0-beta](https://mcr.microsoft.com/product/azure-sdk/azure-mcp) as a remote, HTTPS-accessible MCP server on Azure Container Apps. Clients such as [Microsoft Foundry](https://azure.microsoft.com/products/ai-foundry) agents and [Microsoft Copilot Studio](https://www.microsoft.com/microsoft-copilot/microsoft-copilot-studio) can securely invoke MCP tools that perform Azure operations on the user's behalf via the OBO flow.

## Prerequisites

- Azure subscription with **Owner** or **User Access Administrator** permissions
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)

## Quick Start

```bash
azd up
```

You'll be prompted for an **environment name**, **subscription**, and **resource group**.

This deploys read-only Azure Storage tools over HTTPS. To customize server startup flags, see the [Azure MCP Server docs](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md).

### Post-deployment steps

> **Required** — After `azd up` completes, you must add the server's API scope to the client app registration:
>
> 1. Run `azd env get-values` and note `ENTRA_APP_CLIENT_CLIENT_ID`.
> 2. In the Azure Portal, find the client app registration by that ID.
> 3. Go to **API permissions** → **Add a permission** → **My APIs** tab.
> 4. Select the server app registration and add the `Mcp.Tools.ReadWrite` scope.
> 5. Grant **admin consent** for the added permissions.
>
> If the server app doesn't appear under **My APIs**, see [EntraIdConfig.md](./EntraIdConfig.md).

## What Gets Deployed

| Resource | Purpose |
|---|---|
| **Container App** | Hosts the Azure MCP Server (storage namespace) |
| **User-assigned managed identity** | Client credential for the OBO flow |
| **Entra App Registration (Server)** | OAuth 2.0 auth with `Mcp.Tools.ReadWrite` scope; OBO token exchange for downstream Azure services |
| **Entra App Registration (Client)** | Used by clients (e.g. Power Apps custom connector) to connect to the server |
| **Application Insights** | Telemetry and monitoring (enabled by default) |

> **Note:** Both app registrations are created in the same tenant. The client app is pre-authorized on the server app, bypassing explicit consent. See [known issues](#known-issues) if you prefer explicit consent.

### Deployment Outputs

```bash
azd env get-values
```

Key outputs:

```
CONTAINER_APP_URL="https://..."
ENTRA_APP_CLIENT_CLIENT_ID="<client_app_id>"
ENTRA_APP_SERVER_CLIENT_ID="<server_app_id>"
```

## Entra ID Configuration

The template provisions the following automatically:

- **Federated Identity Credential** on the server app (backed by the user-assigned managed identity)
- **Azure Resource Manager** and **Azure Storage** API scopes on the server app

You only need to **grant admin consent** for the storage API permissions. To add tools beyond storage, add the corresponding API permissions to the server app registration.

See [EntraIdConfig.md](./EntraIdConfig.md) for details on consent, adding API permissions, and managing client credentials.

## Template Structure

| Module | Description |
|---|---|
| `main.bicep` | Orchestrates all resource deployments |
| `entra-app.bicep` | Entra App registrations and federated credentials |
| `aca-infrastructure.bicep` | Container App hosting the MCP Server |
| `aca-storage-managed-identity.bicep` | User-assigned managed identity |
| `application-insights.bicep` | Application Insights (conditional) |

## Test the Server

See [Usage.md](./Usage.md).

## Clean Up

```bash
azd down
```

`azd down` does **not** delete Entra app registrations. Delete them manually in the Azure Portal using the `ENTRA_APP_CLIENT_CLIENT_ID` and `ENTRA_APP_SERVER_CLIENT_ID` values.

To clean up Power Platform resources (custom connector, connection, Copilot Studio agent), use the Power Platform UI.
