using System.Text.Json;
using System.Text;
using PRS.FileHandle;

namespace PRS.Services;

internal class SchemaSearchService
{
    private readonly IFileProvider _fileProvider;

    public SchemaSearchService(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public async Task<string> BuildSchemaContextAsync(string schemaFilePath, string extractionJson)
    {
        // Parse JSON to extract hints
        HashSet<string> tableHints = [];
        HashSet<string> columnHints = [];
        try
        {
            using JsonDocument doc = JsonDocument.Parse(extractionJson ?? "{}");
            if (doc.RootElement.TryGetProperty("candidateTables", out var ct) && ct.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in ct.EnumerateArray())
                {
                    if (v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                        tableHints.Add(Normalize(v.GetString()));
                }
            }
            if (doc.RootElement.TryGetProperty("candidateColumns", out var cc) && cc.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in cc.EnumerateArray())
                {
                    if (v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                        columnHints.Add(Normalize(v.GetString()));
                }
            }
        }
        catch
        {
            // If parsing fails, continue with empty hints
        }

        // Use new Markdown reader API
        using var reader = _fileProvider.GetSchemaReader(schemaFilePath);
        var allTables = await reader.ReadTablesAsync();
        var allColumns = await reader.ReadAllColumnsAsync();

        StringBuilder sb = new();

        // Process tables
        foreach (var table in allTables)
        {
            if (tableHints.Count == 0 || 
                tableHints.Contains(Normalize(table.TableName)) || 
                tableHints.Any(h => table.TableName.Contains(h, StringComparison.OrdinalIgnoreCase)))
            {
                sb.AppendLine($"TABLE|{table.TableSchema}|{table.TableName}|{table.TableType}");
            }
        }

        // Process columns
        foreach (var column in allColumns)
        {
            bool tableOk = tableHints.Count == 0 || 
                           tableHints.Contains(Normalize(column.TableName)) || 
                           tableHints.Any(h => column.TableName.Contains(h, StringComparison.OrdinalIgnoreCase));
            bool colOk = columnHints.Count == 0 || 
                         columnHints.Contains(Normalize(column.ColumnName)) || 
                         columnHints.Any(h => column.ColumnName.Contains(h, StringComparison.OrdinalIgnoreCase));
            
            if (tableOk || colOk)
            {
                sb.AppendLine($"COLUMN|{column.TableSchema}|{column.TableName}|{column.ColumnName}|{column.DataType}");
            }
        }

        return sb.ToString();
    }

    public static bool ValidateSql(string sql, string schemaContext)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;
        if (string.IsNullOrWhiteSpace(schemaContext)) return false;

        // Build allowed tables set from context
        HashSet<string> allowedTables = [];
        foreach (string line in schemaContext.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("TABLE|", StringComparison.Ordinal))
            {
                string[] parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    string schema = parts[1];
                    string table = parts[2];
                    allowedTables.Add($"{schema}.{table}".ToLowerInvariant());
                    allowedTables.Add($"[{schema}].[{table}]".ToLowerInvariant());
                }
            }
        }

        // crude check: ensure every FROM/JOIN table reference is within allowed set
        string lowered = sql.ToLowerInvariant();
        if (!lowered.Contains(" from ")) return true; // allow subqueries etc., minimal check

        // Simple token-based scan
        foreach (string token in new[] { " from ", " join " })
        {
            int idx = 0;
            while ((idx = lowered.IndexOf(token, idx, StringComparison.Ordinal)) >= 0)
            {
                idx += token.Length;
                // capture up to next whitespace or bracket/alias keywords
                int end = lowered.IndexOfAny(" \t\r\n;".ToCharArray(), idx);
                if (end < 0) end = lowered.Length;
                string tableRef = lowered.Substring(idx, end - idx).Trim();
                // strip alias if present like schema.table as t
                tableRef = tableRef.TrimEnd(',');
                if (tableRef.Contains(" as ")) tableRef = tableRef.Split(" as ")[0];
                if (tableRef.Contains(" ")) tableRef = tableRef.Split(' ')[0];
                if (!allowedTables.Contains(tableRef))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string Normalize(string s)
    {
        return s?.Trim().Replace("[", string.Empty).Replace("]", string.Empty).Replace("`", string.Empty).ToLowerInvariant();
    }
}


