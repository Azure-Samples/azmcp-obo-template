# Troubleshooting the container app

If the container app doesn't work as expected, here are a few tools to help diagnose what went wrong.

## Container App Log Stream

In the Azure Portal, open your Container App resource → **Monitoring** → **Log stream** to view real-time app logs.

## Entra Sign-in Logs

The OBO token exchange appears in your tenant's sign-in logs. In the Azure Portal, go to **Microsoft Entra ID** → **Monitoring** → **Sign-in logs** → **User sign-ins (non-interactive)**. Look for entries where:

- **User principal name** = your user account
- **Application** = your server app registration
- **Resource** = the downstream API the MCP tool is accessing

## Application Insights

The template creates an Application Insights resource and wires it to the container app. Two ways to inspect telemetry:

- **Search** — Application Insights → **Investigate** → **Search** for traces.
- **Query** — Application Insights → **Monitoring** → **Logs**. Telemetry is in the `requests` and `traces` tables.

If you'd like additional telemetry points for diagnosing issues, please [open an issue](https://github.com/Azure-Samples/azmcp-obo-template/issues).

## Common Errors

### IDW10502: MsalUiRequiredException

```
{"status":500,"message":"IDW10502: An MsalUiRequiredException was thrown due to a challenge for the user..."}
```

This means the server's OBO token exchange failed because **admin consent has not been granted** for the downstream API permissions on the server app registration.

**Fix:** In the Azure Portal, find the server app registration (using `ENTRA_APP_SERVER_CLIENT_ID`) → **API permissions** → click **Grant admin consent** for all listed permissions (e.g. Azure Resource Manager, Azure Storage).

### ServiceManagementReference field is required

```
{"error":{"code":"BadRequest","target":"/resources/entraApp","message":"ServiceManagementReference field is required for Update..."}}
```

This occurs when redeploying (`azd up`) an existing Entra app registration that was originally created without a `serviceManagementReference`. The Microsoft Graph API now requires this field on updates.

**Fix:** In the Azure Portal, find the app registration → **Branding & properties** → set **Service Management Reference** to a valid GUID, then re-run `azd up`.