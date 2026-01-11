namespace PRS.McpServer.Mcp;

internal interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    object GetToolDefinition();
    Task<object> ExecuteAsync(Dictionary<string, object?> arguments);
}

