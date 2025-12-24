using System.Text;
using PRS.Database;
using PRS.McpServer.Services;

namespace PRS.McpServer.Mcp.Tools;

internal static class OutputFormatter
{
    /// <summary>
    /// Formats table search results for human reading
    /// </summary>
    public static string FormatTables(IEnumerable<TableModel> tables)
    {
        if (!tables.Any())
        {
            return "No tables found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {tables.Count()} table(s):");
        sb.AppendLine();

        foreach (var table in tables)
        {
            sb.AppendLine($"  Schema: {table.TableSchema}");
            sb.AppendLine($"  Table: {table.TableName}");
            sb.AppendLine($"  Type: {table.TableType}");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats column search results for human reading
    /// </summary>
    public static string FormatColumns(IEnumerable<ColumnModel> columns)
    {
        if (!columns.Any())
        {
            return "No columns found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {columns.Count()} column(s):");
        sb.AppendLine();

        foreach (var col in columns)
        {
            sb.AppendLine($"  Schema: {col.TableSchema}");
            sb.AppendLine($"  Table: {col.TableName}");
            sb.AppendLine($"  Column: {col.ColumnName}");
            sb.AppendLine($"  Data Type: {col.DataType}");
            sb.AppendLine($"  Nullable: {col.IsNullable}");
            
            if (!string.IsNullOrWhiteSpace(col.ReferencedTableName))
            {
                sb.AppendLine($"  Foreign Key: {col.ReferencedTableSchema}.{col.ReferencedTableName}.{col.ReferencedColumnName}");
            }
            
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats stored procedure search results for human reading
    /// </summary>
    public static string FormatStoredProcedures(IEnumerable<string> procedures)
    {
        if (!procedures.Any())
        {
            return "No stored procedures found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {procedures.Count()} stored procedure(s):");
        sb.AppendLine();

        foreach (var proc in procedures)
        {
            sb.AppendLine($"  {proc}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats table schema details for human reading
    /// </summary>
    public static string FormatTableSchema(IEnumerable<ColumnModel> columns, string tableName, string schema, bool found)
    {
        if (!found || !columns.Any())
        {
            return $"Table '{tableName}' not found" + (schema != "unknown" ? $" in schema '{schema}'" : "");
        }

        var sb = new StringBuilder();
        var firstColumn = columns.First();
        sb.AppendLine($"Table: {firstColumn.TableSchema}.{firstColumn.TableName}");
        sb.AppendLine($"Columns ({columns.Count()}):");
        sb.AppendLine();

        foreach (var col in columns.OrderBy(c => int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0))
        {
            sb.Append($"  {col.ColumnName}");
            sb.Append($" ({col.DataType}");
            
            if (!string.IsNullOrWhiteSpace(col.CharacterMaximumLength))
            {
                sb.Append($"({col.CharacterMaximumLength})");
            }
            
            sb.Append(")");
            
            if (col.IsNullable == "YES")
            {
                sb.Append(" NULL");
            }
            else
            {
                sb.Append(" NOT NULL");
            }

            if (!string.IsNullOrWhiteSpace(col.ColumnDefault))
            {
                sb.Append($" DEFAULT {col.ColumnDefault}");
            }

            if (!string.IsNullOrWhiteSpace(col.ReferencedTableName))
            {
                sb.AppendLine();
                sb.Append($"    -> FK: {col.ReferencedTableSchema}.{col.ReferencedTableName}.{col.ReferencedColumnName}");
                if (!string.IsNullOrWhiteSpace(col.ForeignKeyName))
                {
                    sb.Append($" ({col.ForeignKeyName})");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats schema list for human reading
    /// </summary>
    public static string FormatSchemas(string? activeSchema, IEnumerable<SchemaInfo> schemas)
    {
        var sb = new StringBuilder();
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
    /// Formats schema switch result for human reading
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
}

