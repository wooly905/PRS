using System.Text.Json;
using PRS.Database;
using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal class FindTableTool : IMcpTool
{
    private readonly SchemaService _schemaService;

    public FindTableTool(SchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public string Name => "find_table";
    public string Description => "Search for tables by keyword (partial match, case-insensitive)";

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
                        description = "Keyword to search for in table names"
                    },
                    output_format = new
                    {
                        type = "string",
                        description = $"Output format: {OutputFormatter.McpSearchFormatEnum} (default: json). Use 'json' for structured data, 'text' for human-readable format",
                        @enum = new[] { "json", "text" }
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
        var format = OutputFormatter.ParseMcpSearchFormat(arguments);
        var tables = await _schemaService.FindTablesAsync(keyword);

        var tableList = tables.Select(t => new
        {
            schema = t.TableSchema,
            name = t.TableName,
            type = t.TableType
        }).ToList();

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = OutputFormatter.FormatTables(tables, format)
                }
            },
            data = new
            {
                tables = tableList
            }
        };
    }
}
