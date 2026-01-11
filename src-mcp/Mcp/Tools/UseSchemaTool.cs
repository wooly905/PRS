using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal class UseSchemaTool : IMcpTool
{
    private readonly SchemaService _schemaService;

    public UseSchemaTool(SchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public string Name => "use_schema";
    public string Description => "Switch the active schema to use for subsequent queries. The schema name should not include the .schema.md extension";

    public object GetToolDefinition()
    {
        return new
        {
            name = Name,
            description = Description,
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    schemaName = new
                    {
                        type = "string",
                        description = "Name of the schema to activate (without .schema.md extension)"
                    }
                },
                required = new[] { "schemaName" }
            }
        };
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object?> arguments)
    {
        if (!arguments.TryGetValue("schemaName", out var schemaNameObj) || schemaNameObj == null)
        {
            throw new ArgumentException("schemaName parameter is required");
        }

        string schemaName = schemaNameObj.ToString() ?? string.Empty;
        var result = await _schemaService.SwitchSchemaAsync(schemaName);

        // Return both human-readable format and structured data for LLM
        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = OutputFormatter.FormatSchemaSwitch(result.Success, result.Message, result.ActiveSchema)
                }
            },
            data = new
            {
                success = result.Success,
                message = result.Message,
                activeSchema = result.ActiveSchema
            }
        };
    }
}

