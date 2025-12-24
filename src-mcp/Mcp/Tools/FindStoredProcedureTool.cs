using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal class FindStoredProcedureTool : IMcpTool
{
    private readonly SchemaService _schemaService;

    public FindStoredProcedureTool(SchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public string Name => "find_stored_procedure";
    public string Description => "Search for stored procedures by keyword (partial match, case-insensitive)";

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
                    keyword = new
                    {
                        type = "string",
                        description = "Keyword to search for in stored procedure names"
                    }
                },
                required = new[] { "keyword" }
            }
        };
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object?> arguments)
    {
        if (!arguments.TryGetValue("keyword", out var keywordObj) || keywordObj == null)
        {
            throw new ArgumentException("keyword parameter is required");
        }

        string keyword = keywordObj.ToString() ?? string.Empty;
        var procedures = await _schemaService.FindStoredProceduresAsync(keyword);

        var procedureList = procedures.ToList();

        // Return both human-readable format and structured data for LLM
        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = OutputFormatter.FormatStoredProcedures(procedureList)
                }
            },
            data = new
            {
                storedProcedures = procedureList
            }
        };
    }
}

