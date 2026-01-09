# Entra ID configuration

## Add API permission

Use the [az ad app permission add](https://learn.microsoft.com/en-us/cli/azure/ad/app/permission?view=azure-cli-latest#az-ad-app-permission-add) Azure CLI command to add additional API permissions.

```bash
az ad app permission add --id {entraAppId} --api {downstreamAppId} --api-permissions {apiPermissionId}=Scope
```

> Note: The Entra app ID can be found in the `azd env get-values` output. 

Azure MCP has tools that access various kinds of downstream APIs. `Azure Resource Manager` and `Azure Storage` are the ones needed for its storage tools. If you aren't sure what downstream API you need for the tool you would like to use, checkout out this [API permission document](tbd) to see which API permissions you need to add for the Azure MCP tools you want to use.

## Grant admin consent for downstream API calls

Use the [az ad app permission admin-consent](https://learn.microsoft.com/en-us/cli/azure/ad/app/permission?view=azure-cli-latest#az-ad-app-permission-admin-consent) Azure CLI command to grant admin consent to the API permission.

```bash
az ad app permission admin-consent --id {entraAppId}
```

If you don't give consent for the API permissions of the downstream APIs, the Azure MCP server will receive a claims challenge saying the user haven't given consent to the downstream API when attempting to acquire an access token for it.

## Create a Client Credential for the server app registration

1. Login to Azure Portal and search for the server app registration client ID.
2. Open the `Certificates & secretes` tab, select `Federated credentials` and click `Add credential`.
3. Select `Managed Identity` in the `Federated credential scenario`.
4. Click `Select a managed identity` and follow the UI to select the managed identity created by this template. Give a descriptive name and description and click `Add`.
5. In the Container App's environment variables, add two environment variables `AzureAd__ClientCredentials__0__SourceType=SignedAssertionFromManagedIdentity` and `AzureAd__ClientCredentials__0__ManagedIdentityClientId={your_managed_identity_client_id}`.
