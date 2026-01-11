using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal class ListSchemasTool : IMcpTool
{
    private readonly SchemaService _schemaService;

    public ListSchemasTool(SchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public string Name => "list_schemas";
    public string Description => "List all available database schemas and show which one is currently active";

    public object GetToolDefinition()
    {
        return new
        {
            name = Name,
            description = Description,
            inputSchema = new
            {
                type = "object",
                properties = new { }
            }
        };
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object?> arguments)
    {
        var result = await _schemaService.ListSchemasAsync();

        var schemaList = result.Schemas.Select(s => new
        {
            name = s.Name,
            fileName = s.FileName,
            isActive = s.IsActive
        }).ToList();

        // Return both human-readable format and structured data for LLM
        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = OutputFormatter.FormatSchemas(result.ActiveSchema, result.Schemas)
                }
            },
            data = new
            {
                activeSchema = result.ActiveSchema,
                schemas = schemaList
            }
        };
    }
}

