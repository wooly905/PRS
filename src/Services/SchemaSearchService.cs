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

        // Read schema file and collect only rows that match hints (active schema only by file)
        var reader = _fileProvider.GetFileReader(schemaFilePath);
        bool inTable = false;
        bool inColumn = false;
        StringBuilder sb = new();

        while (true)
        {
            string line = await reader.ReadLineAsync();
            if (line == null) break;

            if (string.Equals(line, Global.TableSectionName))
            {
                inTable = true; inColumn = false; continue;
            }
            if (string.Equals(line, Global.ColumnSectionName))
            {
                inTable = false; inColumn = true; continue;
            }
            if (line.StartsWith("["))
            {
                inTable = false; inColumn = false; continue;
            }

            if (inTable)
            {
                // format: schema,table,type
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    string table = parts[1];
                    if (tableHints.Count == 0 || tableHints.Contains(Normalize(table)) || tableHints.Any(h => table.Contains(h, StringComparison.OrdinalIgnoreCase)))
                    {
                        sb.AppendLine($"TABLE|{parts[0]}|{parts[1]}|{parts[2]}");
                    }
                }
            }
            else if (inColumn)
            {
                // format: schema,table,column,ordinal,default,isnull,type,maxlen,fk,refSchema,refTable,refColumn
                string[] parts = line.Split(',');
                if (parts.Length >= 7)
                {
                    string table = parts[1];
                    string column = parts[2];
                    bool tableOk = tableHints.Count == 0 || tableHints.Contains(Normalize(table)) || tableHints.Any(h => table.Contains(h, StringComparison.OrdinalIgnoreCase));
                    bool colOk = columnHints.Count == 0 || columnHints.Contains(Normalize(column)) || columnHints.Any(h => column.Contains(h, StringComparison.OrdinalIgnoreCase));
                    if (tableOk || colOk)
                    {
                        sb.AppendLine($"COLUMN|{parts[0]}|{parts[1]}|{parts[2]}|{parts[6]}");
                    }
                }
            }
        }

        reader.Dispose();

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


