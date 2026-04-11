#nullable enable

using System.Text;
using System.Text.Json;
using PRS.Database;

namespace PRS.Formatting;

/// <summary>
/// Shared formatter that produces DDL, JSON, or Text representations of schema data.
/// Used by both CLI and MCP tools.
/// </summary>
public static class SchemaFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ── Tables ───────────────────────────────────────────────────────

    public static string FormatTables(IEnumerable<TableModel> tables, OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => FormatTablesJson(tables),
            OutputFormat.Text => FormatTablesText(tables),
            _ => FormatTablesText(tables)
        };
    }

    private static string FormatTablesJson(IEnumerable<TableModel> tables)
    {
        var list = tables.Select(t => new
        {
            schema = t.TableSchema ?? "",
            name = t.TableName ?? "",
            type = t.TableType ?? ""
        }).ToList();

        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static string FormatTablesText(IEnumerable<TableModel> tables)
    {
        var list = tables.ToList();
        if (list.Count == 0) return "No tables found.";

        var sb = new StringBuilder();
        sb.AppendLine($"Found {list.Count} table(s):");
        sb.AppendLine();

        foreach (var t in list)
        {
            if (!string.IsNullOrEmpty(t.TableSchema))
            {
                sb.AppendLine($"  Schema: {t.TableSchema}");
            }

            sb.AppendLine($"  Table: {t.TableName}");
            sb.AppendLine($"  Type: {t.TableType}");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    // ── Columns (search results across multiple tables) ──────────────

    public static string FormatColumns(IEnumerable<ColumnModel> columns, OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => FormatColumnsJson(columns),
            OutputFormat.Text => FormatColumnsText(columns),
            _ => FormatColumnsText(columns)
        };
    }

    private static string FormatColumnsJson(IEnumerable<ColumnModel> columns)
    {
        var list = columns.Select(c => new
        {
            schema = c.TableSchema ?? "",
            table = c.TableName ?? "",
            column = c.ColumnName ?? "",
            dataType = c.DataType ?? "",
            isNullable = c.IsNullable == "YES",
            ordinalPosition = int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0,
            columnDefault = c.ColumnDefault,
            isPrimaryKey = c.IsPrimaryKey,
            isUnique = c.IsUnique,
            isIdentity = c.IsIdentity,
            foreignKey = string.IsNullOrWhiteSpace(c.ReferencedTableName) ? null : new
            {
                name = c.ForeignKeyName,
                referencedSchema = c.ReferencedTableSchema,
                referencedTable = c.ReferencedTableName,
                referencedColumn = c.ReferencedColumnName
            }
        }).ToList();

        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static string FormatColumnsText(IEnumerable<ColumnModel> columns)
    {
        var list = columns.ToList();
        if (list.Count == 0) return "No columns found.";

        var sb = new StringBuilder();
        sb.AppendLine($"Found {list.Count} column(s):");
        sb.AppendLine();

        foreach (var col in list)
        {
            if (!string.IsNullOrEmpty(col.TableSchema))
            {
                sb.AppendLine($"  Schema: {col.TableSchema}");
            }

            sb.AppendLine($"  Table: {col.TableName}");
            sb.AppendLine($"  Column: {col.ColumnName}");
            sb.AppendLine($"  Data Type: {col.DataType}");
            sb.AppendLine($"  Nullable: {col.IsNullable}");

            if (!string.IsNullOrWhiteSpace(col.ReferencedTableName))
            {
                string fkSchema = !string.IsNullOrEmpty(col.ReferencedTableSchema) ? $"{col.ReferencedTableSchema}." : "";
                sb.AppendLine($"  Foreign Key: {fkSchema}{col.ReferencedTableName}.{col.ReferencedColumnName}");
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    // ── Table Schema (all columns for a single table → CREATE TABLE) ─

    public static string FormatTableSchema(IEnumerable<ColumnModel> columns, string tableName, string? schema, bool found, OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Ddl => FormatTableSchemaDdl(columns, tableName, schema, found),
            OutputFormat.Json => FormatTableSchemaJson(columns, tableName, schema, found),
            OutputFormat.Text => FormatTableSchemaText(columns, tableName, schema, found),
            _ => FormatTableSchemaText(columns, tableName, schema, found)
        };
    }

    private static string FormatTableSchemaDdl(IEnumerable<ColumnModel> columns, string tableName, string? schema, bool found)
    {
        var list = columns.OrderBy(c => int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0).ToList();

        if (!found || list.Count == 0)
        {
            return $"-- Table '{tableName}' not found" + (schema != null && schema != "unknown" ? $" in schema '{schema}'" : "");
        }

        var first = list[0];
        string fullName = !string.IsNullOrEmpty(first.TableSchema) ? $"{first.TableSchema}.{first.TableName}" : first.TableName;

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE {fullName} (");

        // Column definitions
        var columnDefs = new List<string>();
        var constraints = new List<string>();

        foreach (var col in list)
        {
            var def = new StringBuilder();
            def.Append($"    {col.ColumnName} {FormatDataTypeDdl(col)}");

            if (col.IsNullable != "YES")
            {
                def.Append(" NOT NULL");
            }

            if (col.IsIdentity)
            {
                def.Append($" IDENTITY({col.IdentitySeed},{col.IdentityIncrement})");
            }

            if (!string.IsNullOrWhiteSpace(col.ColumnDefault))
            {
                def.Append($" DEFAULT {col.ColumnDefault}");
            }

            columnDefs.Add(def.ToString());

            // Collect constraints
            if (col.IsPrimaryKey)
            {
                constraints.Add($"    CONSTRAINT PK_{first.TableName} PRIMARY KEY ({col.ColumnName})");
            }

            if (col.IsUnique && !col.IsPrimaryKey)
            {
                constraints.Add($"    CONSTRAINT UQ_{first.TableName}_{col.ColumnName} UNIQUE ({col.ColumnName})");
            }

            if (!string.IsNullOrWhiteSpace(col.ReferencedTableName))
            {
                string fkRef = !string.IsNullOrEmpty(col.ReferencedTableSchema)
                    ? $"{col.ReferencedTableSchema}.{col.ReferencedTableName}"
                    : col.ReferencedTableName;
                string fkName = !string.IsNullOrWhiteSpace(col.ForeignKeyName) ? col.ForeignKeyName : $"FK_{first.TableName}_{col.ColumnName}";
                constraints.Add($"    CONSTRAINT {fkName} FOREIGN KEY ({col.ColumnName}) REFERENCES {fkRef}({col.ReferencedColumnName})");
            }
        }

        // Join all lines
        var allLines = new List<string>();
        allLines.AddRange(columnDefs);
        allLines.AddRange(constraints);

        sb.AppendLine(string.Join(",\n", allLines));
        sb.Append(");");

        return sb.ToString();
    }

    private static string FormatTableSchemaJson(IEnumerable<ColumnModel> columns, string tableName, string? schema, bool found)
    {
        var list = columns.OrderBy(c => int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0).ToList();

        if (!found || list.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                tableName,
                schema = schema ?? "unknown",
                found = false
            }, JsonOptions);
        }

        var first = list[0];
        var result = new
        {
            tableName = first.TableName,
            schema = first.TableSchema ?? schema ?? "",
            found = true,
            columns = list.Select(c => new
            {
                name = c.ColumnName ?? "",
                dataType = c.DataType ?? "",
                isNullable = c.IsNullable == "YES",
                ordinalPosition = int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0,
                columnDefault = c.ColumnDefault,
                characterMaximumLength = c.CharacterMaximumLength,
                isPrimaryKey = c.IsPrimaryKey,
                isUnique = c.IsUnique,
                isIdentity = c.IsIdentity,
                foreignKey = string.IsNullOrWhiteSpace(c.ReferencedTableName) ? null : new
                {
                    name = c.ForeignKeyName,
                    referencedSchema = c.ReferencedTableSchema,
                    referencedTable = c.ReferencedTableName,
                    referencedColumn = c.ReferencedColumnName
                }
            }).ToList()
        };

        return JsonSerializer.Serialize(result, JsonOptions);
    }

    private static string FormatTableSchemaText(IEnumerable<ColumnModel> columns, string tableName, string? schema, bool found)
    {
        var list = columns.OrderBy(c => int.TryParse(c.OrdinalPosition, out var pos) ? pos : 0).ToList();

        if (!found || list.Count == 0)
        {
            return $"Table '{tableName}' not found" + (schema != null && schema != "unknown" ? $" in schema '{schema}'" : "");
        }

        var first = list[0];
        var sb = new StringBuilder();
        sb.AppendLine($"Table: {(string.IsNullOrEmpty(first.TableSchema) ? "" : first.TableSchema + ".")}{first.TableName}");
        sb.AppendLine($"Columns ({list.Count}):");
        sb.AppendLine();

        foreach (var col in list)
        {
            sb.Append($"  {col.ColumnName}");
            sb.Append($" ({col.DataType})");

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
                string fkSchema = !string.IsNullOrEmpty(col.ReferencedTableSchema) ? $"{col.ReferencedTableSchema}." : "";
                sb.AppendLine();
                sb.Append($"    -> FK: {fkSchema}{col.ReferencedTableName}.{col.ReferencedColumnName}");
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    // ── Stored Procedures ────────────────────────────────────────────

    public static string FormatStoredProcedures(IEnumerable<string> procedures, OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => FormatStoredProceduresJson(procedures),
            OutputFormat.Text => FormatStoredProceduresText(procedures),
            _ => FormatStoredProceduresText(procedures)
        };
    }

    private static string FormatStoredProceduresJson(IEnumerable<string> procedures)
    {
        return JsonSerializer.Serialize(procedures.ToList(), JsonOptions);
    }

    private static string FormatStoredProceduresText(IEnumerable<string> procedures)
    {
        var list = procedures.ToList();
        if (list.Count == 0) return "No stored procedures found.";

        var sb = new StringBuilder();
        sb.AppendLine($"Found {list.Count} stored procedure(s):");
        sb.AppendLine();

        foreach (var proc in list)
        {
            sb.AppendLine($"  {proc}");
        }

        return sb.ToString().TrimEnd();
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static string FormatDataTypeDdl(ColumnModel col)
    {
        string dataType = col.DataType ?? "unknown";

        if (!string.IsNullOrWhiteSpace(col.CharacterMaximumLength)
            && col.CharacterMaximumLength != "-1"
            && !dataType.Contains('('))
        {
            dataType = $"{dataType}({col.CharacterMaximumLength})";
        }
        else if (col.CharacterMaximumLength == "-1" && !dataType.Contains('('))
        {
            dataType = $"{dataType}(MAX)";
        }

        return dataType;
    }

    /// <summary>
    /// Parses a format string into an OutputFormat enum value.
    /// Returns null if the string is not a recognized format.
    /// </summary>
    public static OutputFormat? ParseFormat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        return value.Trim().ToLowerInvariant() switch
        {
            "table" => OutputFormat.Table,
            "ddl" => OutputFormat.Ddl,
            "json" => OutputFormat.Json,
            "text" => OutputFormat.Text,
            _ => null
        };
    }
}
