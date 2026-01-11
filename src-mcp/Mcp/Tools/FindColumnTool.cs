using System.Text.Json;
using PRS.Database;
using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal class FindColumnTool : IMcpTool
{
    private readonly SchemaService _schemaService;

    public FindColumnTool(SchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public string Name => "find_column";
    public string Description => "Search for columns by keyword across all tables (partial match, case-insensitive)";

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
                        description = "Keyword to search for in column names"
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
        var columns = await _schemaService.FindColumnsAsync(keyword);

        var columnList = columns.Select(c => new
        {
            schema = c.TableSchema,
            table = c.TableName,
            column = c.ColumnName,
            dataType = c.DataType,
            isNullable = c.IsNullable,
            hasForeignKey = !string.IsNullOrWhiteSpace(c.ReferencedTableName),
            foreignKey = string.IsNullOrWhiteSpace(c.ReferencedTableName) ? null : new
            {
                referencedSchema = c.ReferencedTableSchema,
                referencedTable = c.ReferencedTableName,
                referencedColumn = c.ReferencedColumnName
            }
        }).ToList();

        // Return both human-readable format and structured data for LLM
        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = OutputFormatter.FormatColumns(columns)
                }
            },
            data = new
            {
                columns = columnList
            }
        };
    }
}

