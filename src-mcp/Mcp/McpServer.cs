using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PRS.McpServer.Mcp.Resources;
using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp;

internal class McpServer
{
    private readonly SchemaService _schemaService;
    private readonly SchemaResource _schemaResource;
    private readonly Dictionary<string, IMcpTool> _tools;
    private readonly TextWriter? _customOut;

    public McpServer(SchemaService schemaService, SchemaResource schemaResource, TextWriter? customOut = null)
    {
        _schemaService = schemaService;
        _schemaResource = schemaResource;
        _tools = new Dictionary<string, IMcpTool>();
        _customOut = customOut;
    }

    public void RegisterTool(IMcpTool tool)
    {
        _tools[tool.Name] = tool;
    }

    public async Task RunAsync()
    {
        var stdin = Console.OpenStandardInput();
        
        // Use the custom output if provided (original stdout), otherwise use Console.OpenStandardOutput()
        // We use UTF8NoBOM to avoid BOM issues with JSON parsing
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        
        using var reader = new StreamReader(stdin, utf8NoBom, leaveOpen: true);
        using var writer = _customOut ?? new StreamWriter(Console.OpenStandardOutput(), utf8NoBom, leaveOpen: true) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, GetJsonOptions());
                if (request == null)
                    continue;

                var response = await HandleRequestAsync(request);
                if (response != null)
                {
                    var responseJson = JsonSerializer.Serialize(response, GetJsonOptions());
                    await writer.WriteLineAsync(responseJson);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new JsonRpcResponse
                {
                    Id = null,
                    Error = new JsonRpcError
                    {
                        Code = -32700,
                        Message = "Parse error",
                        Data = ex.Message
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse, GetJsonOptions());
                await writer.WriteLineAsync(errorJson);
            }
        }
    }

    private async Task<JsonRpcResponse?> HandleRequestAsync(JsonRpcRequest request)
    {
        // Handle notifications (no id)
        if (request.Id == null)
        {
            if (request.Method == "initialized")
            {
                // Initialization notification received
                return null; // No response for notifications
            }
            return null;
        }

        try
        {
            JsonRpcResponse response;

            switch (request.Method)
            {
                case "initialize":
                    response = await HandleInitializeAsync(request);
                    break;

                case "tools/list":
                    response = HandleToolsList(request);
                    break;

                case "tools/call":
                    response = await HandleToolsCallAsync(request);
                    break;

                case "resources/list":
                    response = HandleResourcesList(request);
                    break;

                case "resources/read":
                    response = await HandleResourcesReadAsync(request);
                    break;

                default:
                    response = new JsonRpcResponse
                    {
                        Id = request.Id,
                        Error = new JsonRpcError
                        {
                            Code = -32601,
                            Message = "Method not found"
                        }
                    };
                    break;
            }

            return response;
        }
        catch (Exception ex)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32603,
                    Message = "Internal error",
                    Data = ex.Message
                }
            };
        }
    }

    private async Task<JsonRpcResponse> HandleInitializeAsync(JsonRpcRequest request)
    {
        var initParams = JsonSerializer.Deserialize<InitializeParams>(
            request.Params?.ToString() ?? "{}",
            GetJsonOptions());

        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new InitializeResult
            {
                ProtocolVersion = "2024-11-05",
                Capabilities = new ServerCapabilities
                {
                    Tools = new ToolsCapability(),
                    Resources = new ResourcesCapability()
                },
                ServerInfo = new ServerInfo
                {
                    Name = "prs-mcp",
                    Version = "1.0.0"
                }
            }
        };
    }

    private JsonRpcResponse HandleToolsList(JsonRpcRequest request)
    {
        var tools = _tools.Values.Select(t => t.GetToolDefinition()).ToList();

        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new { tools }
        };
    }

    private async Task<JsonRpcResponse> HandleToolsCallAsync(JsonRpcRequest request)
    {
        var callParams = JsonSerializer.Deserialize<ToolsCallParams>(
            request.Params?.ToString() ?? "{}",
            GetJsonOptions());

        if (callParams == null || string.IsNullOrWhiteSpace(callParams.Name))
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32602,
                    Message = "Invalid params",
                    Data = "Tool name is required"
                }
            };
        }

        if (!_tools.TryGetValue(callParams.Name, out var tool))
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32601,
                    Message = "Tool not found",
                    Data = $"Tool '{callParams.Name}' is not available"
                }
            };
        }

        try
        {
            var result = await tool.ExecuteAsync(callParams.Arguments ?? new Dictionary<string, object?>());
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32603,
                    Message = "Tool execution error",
                    Data = ex.Message
                }
            };
        }
    }

    private JsonRpcResponse HandleResourcesList(JsonRpcRequest request)
    {
        var resources = _schemaResource.ListResources();
        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new { resources }
        };
    }

    private async Task<JsonRpcResponse> HandleResourcesReadAsync(JsonRpcRequest request)
    {
        var readParams = JsonSerializer.Deserialize<ResourcesReadParams>(
            request.Params?.ToString() ?? "{}",
            GetJsonOptions());

        if (readParams == null || string.IsNullOrWhiteSpace(readParams.Uri))
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32602,
                    Message = "Invalid params",
                    Data = "URI is required"
                }
            };
        }

        try
        {
            var result = await _schemaResource.ReadAsync(readParams.Uri);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32603,
                    Message = "Resource read error",
                    Data = ex.Message
                }
            };
        }
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

// JSON-RPC Models
internal class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

internal class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }
}

internal class JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

internal class InitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public object? Capabilities { get; set; }

    [JsonPropertyName("clientInfo")]
    public object? ClientInfo { get; set; }
}

internal class InitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();
}

internal class ServerCapabilities
{
    [JsonPropertyName("tools")]
    public ToolsCapability Tools { get; set; } = new();

    [JsonPropertyName("resources")]
    public ResourcesCapability Resources { get; set; } = new();
}

internal class ToolsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

internal class ResourcesCapability
{
    [JsonPropertyName("subscribe")]
    public bool Subscribe { get; set; } = false;

    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

internal class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

internal class ToolsCallParams
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object?>? Arguments { get; set; }
}

internal class ResourcesReadParams
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

