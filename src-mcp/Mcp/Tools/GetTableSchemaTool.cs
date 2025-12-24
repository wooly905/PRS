using PRS.Database;
using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal class GetTableSchemaTool : IMcpTool
{
    private readonly SchemaService _schemaService;

    public GetTableSchemaTool(SchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public string Name => "get_table_schema";
    public string Description => "Get the complete schema information for a specific table, including all columns with their data types, nullability, and foreign key relationships";

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
                    tableName = new
                    {
                        type = "string",
                        description = "Name of the table"
                    },
                    schema = new
                    {
                        type = "string",
                        description = "Optional schema name (e.g., 'dbo'). If not provided, will search in all schemas"
                    }
                },
                required = new[] { "tableName" }
            }
        };
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object?> arguments)
    {
        if (!arguments.TryGetValue("tableName", out var tableNameObj) || tableNameObj == null)
        {
            throw new ArgumentException("tableName parameter is required");
        }

        string tableName = tableNameObj.ToString() ?? string.Empty;
        string? schema = arguments.TryGetValue("schema", out var schemaObj) && schemaObj != null
            ? schemaObj.ToString()
            : null;

        var columns = await _schemaService.GetTableDetailsAsync(tableName, schema);

        if (!columns.Any())
        {
            var notFoundMessage = $"Table '{tableName}' not found" + (schema != null ? $" in schema '{schema}'" : "");
            return new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = notFoundMessage
                    }
                },
                data = new
                {
                    tableName = tableName,
                    schema = schema ?? "unknown",
                    found = false,
                    message = notFoundMessage
                }
            };
        }

        var firstColumn = columns.First();
        var schemaName = firstColumn.TableSchema;
        var columnList = columns.Select(c => new
        {
            name = c.ColumnName,
            dataType = c.DataType,
            isNullable = c.IsNullable == "YES",
            ordinalPosition = int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0,
            columnDefault = c.ColumnDefault,
            characterMaximumLength = c.CharacterMaximumLength,
            foreignKey = string.IsNullOrWhiteSpace(c.ReferencedTableName) ? null : new
            {
                name = c.ForeignKeyName,
                referencedSchema = c.ReferencedTableSchema,
                referencedTable = c.ReferencedTableName,
                referencedColumn = c.ReferencedColumnName
            }
        }).OrderBy(c => c.ordinalPosition).ToList();

        // Return both human-readable format and structured data for LLM
        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = OutputFormatter.FormatTableSchema(columns, tableName, schemaName, true)
                }
            },
            data = new
            {
                tableName = firstColumn.TableName,
                schema = schemaName,
                found = true,
                columns = columnList
            }
        };
    }
}

