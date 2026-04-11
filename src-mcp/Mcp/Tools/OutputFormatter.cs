using PRS.Database;
using PRS.Formatting;
using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal static class OutputFormatter
{
    /// <summary>
    /// Valid format values for MCP search tools (ddl is excluded — only meaningful for single-table schema).
    /// </summary>
    public const string McpSearchFormatEnum = "json, text";

    /// <summary>
    /// Valid format values for MCP get_table_schema tool (ddl is the default).
    /// </summary>
    public const string McpSchemaFormatEnum = "ddl, json, text";

    /// <summary>
    /// Formats table search results using the specified format.
    /// </summary>
    public static string FormatTables(IEnumerable<TableModel> tables, OutputFormat format = OutputFormat.Json)
    {
        return SchemaFormatter.FormatTables(tables, format);
    }

    /// <summary>
    /// Formats column search results using the specified format.
    /// </summary>
    public static string FormatColumns(IEnumerable<ColumnModel> columns, OutputFormat format = OutputFormat.Json)
    {
        return SchemaFormatter.FormatColumns(columns, format);
    }

    /// <summary>
    /// Formats stored procedure search results using the specified format.
    /// </summary>
    public static string FormatStoredProcedures(IEnumerable<string> procedures, OutputFormat format = OutputFormat.Json)
    {
        return SchemaFormatter.FormatStoredProcedures(procedures, format);
    }

    /// <summary>
    /// Formats table schema details using the specified format.
    /// </summary>
    public static string FormatTableSchema(IEnumerable<ColumnModel> columns, string tableName, string? schema, bool found, OutputFormat format = OutputFormat.Ddl)
    {
        return SchemaFormatter.FormatTableSchema(columns, tableName, schema, found, format);
    }

    /// <summary>
    /// Formats schema list for human reading (no format variants for meta-operations).
    /// </summary>
    public static string FormatSchemas(string? activeSchema, IEnumerable<SchemaInfo> schemas)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Active Schema: {activeSchema ?? "None"}");
        sb.AppendLine();
        sb.AppendLine($"Available Schemas ({schemas.Count()}):");
        sb.AppendLine();

        foreach (var schema in schemas)
        {
            var marker = schema.IsActive ? " * " : "   ";
            sb.AppendLine($"{marker}{schema.Name} ({schema.FileName})");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats schema switch result for human reading.
    /// </summary>
    public static string FormatSchemaSwitch(bool success, string message, string? activeSchema)
    {
        if (success)
        {
            return $"Successfully switched to schema: {activeSchema}\n{message}";
        }
        else
        {
            return $"Failed to switch schema: {message}";
        }
    }

    /// <summary>
    /// Parses the output_format argument for search tools.
    /// Default is JSON. DDL is rejected (falls back to JSON).
    /// </summary>
    public static OutputFormat ParseMcpSearchFormat(Dictionary<string, object?> arguments)
    {
        if (arguments.TryGetValue("output_format", out var formatObj) && formatObj != null)
        {
            var parsed = SchemaFormatter.ParseFormat(formatObj.ToString());
            if (parsed.HasValue && parsed.Value != OutputFormat.Table && parsed.Value != OutputFormat.Ddl)
            {
                return parsed.Value;
            }
        }

        return OutputFormat.Json;
    }

    /// <summary>
    /// Parses the output_format argument for get_table_schema tool.
    /// Default is DDL (CREATE TABLE), optimal for LLM consumption.
    /// </summary>
    public static OutputFormat ParseMcpSchemaFormat(Dictionary<string, object?> arguments)
    {
        if (arguments.TryGetValue("output_format", out var formatObj) && formatObj != null)
        {
            var parsed = SchemaFormatter.ParseFormat(formatObj.ToString());
            if (parsed.HasValue && parsed.Value != OutputFormat.Table)
            {
                return parsed.Value;
            }
        }

        return OutputFormat.Ddl;
    }
}
