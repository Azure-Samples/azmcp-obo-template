# Azure MCP Server - remotely hosted in Azure Container App with On-Behalf-Of flow

This document explains how to deploy the [Azure MCP Server 2.0-beta](https://mcr.microsoft.com/product/azure-sdk/azure-mcp) as a remote MCP server accessible over HTTPS. This enables various kinds of MCP clients, such as AI agents from [Microsoft Foundry](https://azure.microsoft.com/products/ai-foundry) and [Microsoft Copilot Studio](https://www.microsoft.com/microsoft-copilot/microsoft-copilot-studio) to securely invoke MCP tools that perform Azure operations on your behalf.

This MCP server will use On-Behalf-Of (OBO) flow when executing its tools. This means it will use the user credential of the access token that was used to access it to make downstream API calls to various Azure services. If you would like to host a centralized Azure MCP server for users with various levels of permissions, this hosting option will be a good fit.

## Prerequisite

- An Azure subscription with **Owner** or **User Access Administrator** permissions
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)

## Template Structure

The `azd` template consists of the following Bicep modules:

- **`main.bicep`** - Orchestrates the deployment of all resources.
- **`aca-storage-managed-identity.bicep`** - Create a user-assigned managed identity
- **`aca-infrastructure.bicep`** - Deploys Container App hosting the Azure MCP Server.
- **`entra-app.bicep`** - Creates Entra App registrations.
- **`application-insights.bicep`** - Deploys Application Insights for telemetry and monitoring (conditional deployment, enabled by default).

## Quick start

This reference template deploys the Azure MCP Server with **read-only** Azure Storage tools enabled, accessible over HTTPS transport. For details on customizing server startup flags and configuration, see [Azure MCP Server documentation](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md).

```bash
azd up
```

For the first time running this command, you will be prompted to sign in to your Azure account and give the following information:

- **Environment Name** - A user friendly name for managing azd deployments.
- **Azure Subscription** - The Azure subscription in which to create the resources.
- **Resource Group** - The resource group in which to create the resources. You can create a new resource group on demand during this step.

## What gets deployed

- **Container App** - Runs Azure MCP Server with storage namespace.
- **User-assigned managed identity** - A user-assigned managed identity assigned to the container app. This managed identity will be used as the client credential for the OBO flow. 
- **Entra App Registration (Azure MCP Server)** - For incoming OAuth 2.0 authentication from clients (e.g. custom connector) with `Mcp.Tools.ReadWrite` scope. This scope will be requested by the client app accessing the server. Also used for OBO flow to exchange access tokens for downstream Azure services.
- **Entra App Registration (Client)** - For clients, like Power Apps custom connector, to connect to the remote Azure MCP Server.
- **Application Insights** - Telemetry and monitoring.

> Note: this template creates the server app registration and the client app registration in the same tenant. It also sets the client app as a pre-authorized app of the server app. This allows it to bypass the explicit consent flow when using the client app to access the server app. If you prefer explicitly giving consent for the client app, please refer to [known issues](#known-issues) for available options.

### Deployment outputs

After deployment, retrieve `azd` outputs:

```bash
azd env get-values
```

Among the output there are useful values for the subsequent steps. Here is an example of these values.

```
AZURE_RESOURCE_GROUP="<your_resource_group_name>"
AZURE_SUBSCRIPTION_ID="<your_subscription_id>"
AZURE_TENANT_ID="<your_tenant_id>"
CONTAINER_APP_NAME="azure-mcp-storage-server"
CONTAINER_APP_URL="https://azure-mcp-storage-server.whitewave-ff25acf5.westus3.azurecontainerapps.io"
ENTRA_APP_CLIENT_CLIENT_ID="<your_client_app_registration_client_id>"
ENTRA_APP_SERVER_CLIENT_ID="<your_server_app_registration_client_id>"
```

You also need to add the created API scope as one of the permissions of the client app registration. Go to Azure Portal and search for the client app registration using the ENTRA_APP_CLIENT_CLIENT_ID output value. In the `API permissions` blade, click `Add a permission`, select server app registration in the `My APIs` tab and add the 'Mcp.Tools.ReadWrite' scope.

> Note: if your server app registration doesn't show up in the `My APIs` tab, refer to [EntraIdConfig.md](./EntraIdConfig.md) on how to manually add its API scope to the client app registration.

## Entra ID configuration

Since the self-hosted Azure MCP server need to exchange incoming user access tokens for access tokens used by downstream APIs, the server app registration must have valid client credentials to identify itself to the MS Entra platform and add the necessary downstream API scopes to its manifest. 

The `entraApp.bicep` template has already included a Federated Identity Credential backed by a user assigned managed identity provisioned in the same template, and the `Azure Resource Manager` and `Azure Storage` API scopes to the server app registration for the storage tools. The `aca-infrastructure.bicep` template has added the necessary environment variables to let the MCP server pick the correct client credential. You only need to grant the admin consent for the already added storage tools in this template to work. However, if you need the MCP server to call other tools, you will need to add additional API permissions for them to work.

Refer to [EntraIdConfig.md](./EntraIdConfig.md) on how to give consent to the API permissions, add additional API permissions, and add additional client credentials.

## Test the Server

Refer to [Usage.md](./Usage.md) on how you may use MCP clients to test the functionality of the MCP server.

## Clean Up

If you no longer need the Azure resources created by this template, run this command to delete them.

```bash
azd down
```

Azd cannot delete the Entra app registrations created by this template. You can delete the Entra app registrations created by this template by searching for the `ENTRA_APP_CLIENT_CLIENT_ID` and the `ENTRA_APP_SERVER_CLIENT_ID` in Azure Portal and deleting the corresponding app registrations in Portal UI.

If you need to clean up the Power Platform resources, please use the Power Platform UI to delete the Copilot Studio Agent, Power Apps custom connector and connection.
