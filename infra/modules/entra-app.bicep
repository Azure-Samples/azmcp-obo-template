/*
  This template creates an Entra (Azure AD) application with the necessary components
  for secure authentication and authorization in Azure.

  What gets created:

  Entra Application Registration
     This is like a "blueprint" that defines what the Entra App can do. It includes
     API scopes, identifier URIs for OAuth validation,
     and basic app configuration.
*/

extension microsoftGraphV1

@description('Display name for the Entra Application')
param entraAppDisplayName string

@description('Unique name for the Entra Application')
param entraAppUniqueName string

param isServer bool

@description('Value of the app scope')
param entraAppScopeValue string = ''

@description('Display name of the app scope')
param entraAppScopeDisplayName string = ''

@description('Description of the app scope')
param entraAppScopeDescription string = ''

@description('Known client app id')
param knownClientAppId string = ''

@description('Object ID of the container app user assigned managed identity')
param acaManagedIdentityObjectId string = ''

var scopeId = guid(entraAppUniqueName, entraAppScopeValue)

resource entraApp 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: entraAppUniqueName 
  displayName: entraAppDisplayName
  serviceManagementReference: 'a49bf0bf-6e3d-487d-8646-938d84115657'
  api: isServer ? {
    oauth2PermissionScopes: [
      {
        id: scopeId
        type: 'User'
        adminConsentDescription: entraAppScopeDescription
        adminConsentDisplayName: entraAppScopeDisplayName
        userConsentDescription: entraAppScopeDescription
        userConsentDisplayName: entraAppScopeDisplayName
        value: entraAppScopeValue
        isEnabled: true
      }
    ]
    preAuthorizedApplications: [
      {
        appId: knownClientAppId
        delegatedPermissionIds: [
          scopeId
        ]
      }
    ]
    requestedAccessTokenVersion: 2
  } : null
  requiredResourceAccess: isServer ? [
    {
      // Azure Resource Manager API permission
			resourceAppId: '797f4846-ba00-4fd7-ba43-dac1f8f63013'
			resourceAccess: [
				{
					id: '41094075-9dad-400e-a0bd-54e686782033'
					type: 'Scope'
				}
			]
		}
    {
      // Azure Storage data plane API permission
      resourceAccess: [
        {
          id: '03e0da56-190b-40ad-a80c-ea378c433f7f'
          type: 'Scope'
        }
      ]
      resourceAppId: 'e406a681-f3d4-42a8-90b6-c2b029497af1'
    }
  ] : []
  
  resource federatedIdentityCredential 'federatedIdentityCredentials@v1.0' = if (isServer) {
    audiences: [
      'api://AzureADTokenExchange'
    ]
    description: 'Client credential of Azure MCP server app registration'
    issuer: '${environment().authentication.loginEndpoint}${tenant().tenantId}/v2.0'
    name: 'ServerClientCredential'
    subject: acaManagedIdentityObjectId
  }
}

output entraAppClientId string = entraApp.appId
output entraAppObjectId string = entraApp.id
output entraAppIdentifierUri string = 'api://${entraApp.appId}'
output entraAppScopeValue string = entraAppScopeValue
output entraAppScopeId string = isServer ? entraApp.api.oauth2PermissionScopes[0].id : ''
