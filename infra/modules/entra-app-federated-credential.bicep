/*
  Template to create a Federated Identity Credential (FIC) for an existing Entra ID App.
*/

extension microsoftGraphV1

@description('The unique name of the existing Entra App')
param entraAppUniqueName string

@description('Object ID of the container app user assigned managed identity')
param acaManagedIdentityObjectId string

// Reference the existing Entra ID App by its uniqueName
resource existingEntraApp 'Microsoft.Graph/applications@v1.0' existing = {
  uniqueName: entraAppUniqueName
}

// Create federated identity credential for the Entra ID App
resource federatedIdentityCredential 'Microsoft.Graph/applications/federatedIdentityCredentials@v1.0' = {
  name: '${existingEntraApp.uniqueName}/ServerClientCredential'
  audiences: [
    'api://AzureADTokenExchange'
  ]
  description: 'Client credential of Azure MCP server app registration'
  issuer: '${environment().authentication.loginEndpoint}${tenant().tenantId}/v2.0'
  subject: acaManagedIdentityObjectId
}

output federatedCredentialName string = federatedIdentityCredential.name
