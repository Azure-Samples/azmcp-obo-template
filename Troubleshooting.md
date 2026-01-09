# Troubleshooting the container app

If the container app doesn't work as expected, here are a few tools to help diagnose what went wrong.

## Getting logs from the container app log stream

The log stream of the container app has a lot of useful data about the status of the app. To access the log stream, open the container app resource in Azure Portal and navigate to the `Monitoring/Log stream` tab.

## Getting logs from Entra sign-in logs

As a part of the OBO flow, Azure MCP will exchange the incoming access token for another access token for the downstream API. This event can be seen in the sign-in logs of the Entra tenant. To access the sign-in logs of the tenant, go to `Microsoft Entra ID` in Azure Portal and navigate to the `Monitoring/Sign-in logs/User sign-ins (non-interactive)` tab. If the self-hosted Azure MCP sever did initiate a flow to exchange the token, there will be an entry where the `User principal name` is your user account used to access the Azure MCP server, the application is the server app registration of your self-hosted Azure MCP server, and the resource is the downstream API that the Azure MCP tool needs to access.

## Getting logs from Application Insights

This template creates an Application Insights resource and configures the container app to send telemetry data to it. The built-in telemetry data contains useful data such as the requests made to the server. There are two common ways to look at the telemetry data.

- Search Application Insights trace. Go to the Application Insights resource in Azure Portal and navigate to the `Investigate/Search` tab. You can start a search to get traces from the server.
- Query logs. Go to the Application Insights resource in Azure Portal and navigate to the `Monitoring/Logs` tab. You can write queries to get data from the logs. By default, telemetry data will be written to the `requests` table and the `traces` table.

If you believe there are additional telemetry points that can further help diagnose service issues, please open an issue to let us know.