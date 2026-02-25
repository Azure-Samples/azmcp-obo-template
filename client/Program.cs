// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

// main:start
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{environment}.json", optional: true)
        .AddCommandLine(args)
        .Build();

var listAccounts = configuration.GetValue<bool?>("list-accounts") ?? false;

var mcpServerUrl = GetMcpServerUrl(configuration);
Console.WriteLine($"MCP Server: {mcpServerUrl}");

var (scopes, tenantId) = await GetOAuthProtectedResourceMetadataAsync(mcpServerUrl);
var clientId = configuration["EntraClientClientId"];

if (string.IsNullOrEmpty(clientId))
{
    throw new InvalidOperationException("EntraClientClientId is not configured");
}

var accessToken = await GetAccessTokenAsync(clientId, tenantId, scopes);

var client = new HttpClient
{
    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
};

var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(mcpServerUrl),
    TransportMode = HttpTransportMode.StreamableHttp
}, client);

var clientOptions = new McpClientOptions
{
    ClientInfo = new()
    {
        Name = "azmcp dev inner loop client",
        Version = "1.0.0"
    }
};

await using var mcpClient = await McpClient.CreateAsync(transport, clientOptions);

Console.WriteLine("Available Tools");

var tools = await mcpClient.ListToolsAsync();
if (tools.Count == 0)
{
    Console.WriteLine("No tools available.");
}
else
{
    foreach (var tool in tools)
    {
        Console.WriteLine($"\nTool: {tool.Name}");
        Console.WriteLine($"  Description: {tool.Description}");

        if (tool.JsonSchema.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine($"  Schema: {tool.JsonSchema}");
        }
    }
}
// main:end

// tool-call:start
if (listAccounts)
{
    Console.WriteLine("\n--- Calling storage_account_get Tool ---");

    var subscriptionId = configuration["SubscriptionId"];
    if (string.IsNullOrEmpty(subscriptionId))
    {
        throw new InvalidOperationException("SubscriptionId is not configured");
    }

    var toolCallArgs = new Dictionary<string, object?>
    {
        { "subscription", subscriptionId }
    };

    var toolResult = await mcpClient.CallToolAsync("storage_account_get", toolCallArgs);
    Console.WriteLine($"Tool Result:");
    foreach (var content in toolResult.Content)
    {
        Console.WriteLine($"  Type: {content.Type}");
        Console.WriteLine($"  Data: {JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true })}");
    }
}

// tool-call:end

static string GetMcpServerUrl(IConfiguration configuration)
{
    var mcpServerUrl = configuration["McpServer:Url"];
    if (string.IsNullOrEmpty(mcpServerUrl))
    {
        throw new InvalidOperationException("McpServer:Url is not configured");
    }

    return mcpServerUrl;
}

static async Task<(string[] Scopes, string TenantId)> GetOAuthProtectedResourceMetadataAsync(string mcpServerUrl)
{
    using var httpClient = new HttpClient();
    var endpoint = $"{mcpServerUrl.TrimEnd('/')}/.well-known/oauth-protected-resource";

    var response = await httpClient.GetStringAsync(endpoint);
    var metadata = JsonSerializer.Deserialize<OAuthProtectedResourceMetadata>(response);
    if (metadata is null)
    {
        throw new InvalidOperationException("Failed to parse OAuth Protected Resource Metadata");
    }

    if (metadata.ScopesSupported is null || metadata.ScopesSupported.Count == 0)
    {
        throw new InvalidOperationException("No scopes found in metadata");
    }
    var scopes = metadata.ScopesSupported.ToArray();

    var authServer = metadata.AuthorizationServers?.FirstOrDefault();
    if (authServer is null)
    {
        throw new InvalidOperationException("No authorization server found in metadata");
    }

    var authServerUri = new Uri(authServer);
    if (authServerUri.Segments.Length < 2)
    {
        // format: https://login.microsoftonline.com/{tenant-id}/v2.0
        throw new InvalidOperationException($"Invalid authorization server URL format: {authServer}");
    }
    var tenantId = authServerUri.Segments[1].TrimEnd('/');

    return (scopes, tenantId);
}

static async Task<string> GetAccessTokenAsync(string clientId, string tenantId, string[] scopes)
{
    Console.WriteLine($"Acquiring access token for scope(s) '{string.Join(", ", scopes)}' with tenant '{tenantId}'");

    var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
    {
        ClientId = clientId,
        TenantId = tenantId
    });

    var requestContext = new Azure.Core.TokenRequestContext(scopes);
    var accessToken = await credential.GetTokenAsync(requestContext);

    Console.WriteLine($"Token acquired, expires: {accessToken.ExpiresOn}");

    return accessToken.Token;
}

// OAuth Protected Resource Metadata schema
// PRM document at https://azure-mcp-remote-server.stranger-things.eastus2.azurecontainerapps.io/.well-known/oauth-protected-resource
internal class OAuthProtectedResourceMetadata
{
    // https://azure-mcp-remote-server.stranger-things.eastus2.azurecontainerapps.io
    [JsonPropertyName("resource")]
    public string? Resource { get; set; }

    // format: https://login.microsoftonline.com/{tenant-id}/v2.0
    // e.g.  : https://login.microsoftonline.com/70a036f6-8e4d-4615-bad6-149c02e7720d/v2.0
    [JsonPropertyName("authorization_servers")]
    public List<string>? AuthorizationServers { get; set; }

    // format: {client-id}/{scope-name}
    // e.g.  : 0dfbc09b-6dd8-4de3-ae48-054c806ee59f/Mcp.Tools.ReadWrite
    [JsonPropertyName("scopes_supported")]
    public List<string>? ScopesSupported { get; set; }

    // header
    [JsonPropertyName("bearer_methods_supported")]
    public List<string>? BearerMethodsSupported { get; set; }

    // https://github.com/Microsoft/mcp
    [JsonPropertyName("resource_documentation")]
    public string? ResourceDocumentation { get; set; }
}