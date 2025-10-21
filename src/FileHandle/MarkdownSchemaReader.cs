using System.Text.RegularExpressions;
using PRS.Database;

namespace PRS.FileHandle;

/// <summary>
/// Reads database schema from Markdown format with key-value structure.
/// Supports partial string search for tables and columns.
/// </summary>
internal class MarkdownSchemaReader : ISchemaReader
{
    private readonly string _filePath;
    private readonly List<string> _lines;
    private readonly Dictionary<string, List<ColumnModel>> _tableColumns;
    private readonly List<TableModel> _tables;
    private readonly List<string> _storedProcedures;
    private string _connectionString;

    public MarkdownSchemaReader(string filePath)
    {
        _filePath = filePath;
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Schema file not found: {filePath}");
        }

        _lines = File.ReadAllLines(filePath).ToList();
        _tableColumns = new Dictionary<string, List<ColumnModel>>(StringComparer.OrdinalIgnoreCase);
        _tables = new List<TableModel>();
        _storedProcedures = new List<string>();

        ParseMarkdownContent();
    }

    public Task<string> ReadConnectionStringAsync()
    {
        return Task.FromResult(_connectionString);
    }

    public Task<IEnumerable<TableModel>> ReadTablesAsync()
    {
        return Task.FromResult<IEnumerable<TableModel>>(_tables);
    }

    public Task<IEnumerable<ColumnModel>> ReadAllColumnsAsync()
    {
        var allColumns = _tableColumns.Values.SelectMany(cols => cols).ToList();
        return Task.FromResult<IEnumerable<ColumnModel>>(allColumns);
    }

    public Task<IEnumerable<ColumnModel>> ReadColumnsForTableAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        // First try exact match (schema.table)
        var key = _tableColumns.Keys.FirstOrDefault(k => 
            string.Equals(k, tableName, StringComparison.OrdinalIgnoreCase));
        
        if (key != null && _tableColumns.TryGetValue(key, out var columns))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(columns);
        }

        // If no exact match, try to find by table name only
        key = _tableColumns.Keys.FirstOrDefault(k => 
            k.EndsWith($".{tableName}", StringComparison.OrdinalIgnoreCase));
        
        if (key != null && _tableColumns.TryGetValue(key, out columns))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(columns);
        }

        return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
    }

    public Task<IEnumerable<string>> ReadStoredProceduresAsync()
    {
        return Task.FromResult<IEnumerable<string>>(_storedProcedures);
    }

    public Task<IEnumerable<TableModel>> FindTablesAsync(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
        {
            return Task.FromResult<IEnumerable<TableModel>>(new List<TableModel>());
        }

        var matchingTables = _tables.Where(t => 
            t.TableName.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        return Task.FromResult<IEnumerable<TableModel>>(matchingTables);
    }

    public Task<IEnumerable<ColumnModel>> FindColumnsAsync(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        var matchingColumns = _tableColumns.Values
            .SelectMany(cols => cols)
            .Where(c => c.ColumnName.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        return Task.FromResult<IEnumerable<ColumnModel>>(matchingColumns);
    }

    private void ParseMarkdownContent()
    {
        string connectionString = string.Empty;
        bool inStoredProceduresSection = false;
        string currentTableName = string.Empty;
        string currentTableSchema = string.Empty;
        string currentTableType = string.Empty;
        List<ColumnModel> currentColumns = new();

        foreach (var line in _lines)
        {
            var trimmedLine = line.Trim();

            // Parse connection string
            if (trimmedLine.Equals("```") && connectionString == string.Empty)
            {
                var nextLineIndex = _lines.IndexOf(line) + 1;
                if (nextLineIndex < _lines.Count)
                {
                    var nextLine = _lines[nextLineIndex].Trim();
                    if (!nextLine.Equals("```"))
                    {
                        connectionString = nextLine;
                    }
                }
                continue;
            }

            // Skip closing code block
            if (trimmedLine.Equals("```"))
            {
                continue;
            }

            // Parse stored procedures section
            if (trimmedLine.Equals("## Stored Procedures", StringComparison.OrdinalIgnoreCase))
            {
                inStoredProceduresSection = true;
                continue;
            }

            if (inStoredProceduresSection)
            {
                if (trimmedLine.StartsWith("- "))
                {
                    var procedureName = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrWhiteSpace(procedureName))
                    {
                        _storedProcedures.Add(procedureName);
                    }
                }
                else if (trimmedLine.StartsWith("#") || trimmedLine.StartsWith("##"))
                {
                    inStoredProceduresSection = false;
                }
            }

            // Parse tables section
            if (trimmedLine.StartsWith("### "))
            {
                // Save previous table if exists
                if (!string.IsNullOrWhiteSpace(currentTableName))
                {
                    SaveCurrentTable(currentTableSchema, currentTableName, currentTableType, currentColumns);
                }

                // Parse new table header: ### schema.table
                var tableHeader = trimmedLine.Substring(4).Trim();
                var parts = tableHeader.Split('.');
                currentTableSchema = parts.Length > 1 ? parts[0] : "dbo";
                currentTableName = parts.Length > 1 ? parts[1] : parts[0];
                currentTableType = "BASE TABLE"; // Default
                currentColumns = new List<ColumnModel>();
            }

            // Parse table type
            if (trimmedLine.StartsWith("- **Type**: "))
            {
                currentTableType = trimmedLine.Substring(12).Trim();
            }

            // Parse columns
            if (trimmedLine.StartsWith("- **Columns**:"))
            {
                // Column list starts on next line
                continue;
            }

            // Parse individual column
            if (line.TrimStart().StartsWith("- ") && !string.IsNullOrWhiteSpace(currentTableName))
            {
                var columnLine = line.TrimStart().Substring(2); // Remove "- " from the start
                
                // Check if next line contains FK information
                var currentIndex = _lines.IndexOf(line);
                string fkInfo = string.Empty;
                if (currentIndex + 1 < _lines.Count)
                {
                    var nextLine = _lines[currentIndex + 1].Trim();
                    if (nextLine.StartsWith("- **FK**: "))
                    {
                        fkInfo = nextLine.Substring(10); // Remove "- **FK**: "
                    }
                }
                
                var column = ParseColumn(columnLine, fkInfo, currentTableSchema, currentTableName);
                if (column != null && !string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    currentColumns.Add(column);
                }
            }
        }

        // Save last table
        if (!string.IsNullOrWhiteSpace(currentTableName))
        {
            SaveCurrentTable(currentTableSchema, currentTableName, currentTableType, currentColumns);
        }

        _connectionString = connectionString;
    }

    private ColumnModel ParseColumn(string columnLine, string fkInfo, string tableSchema, string tableName)
    {
        // Format: ColumnName (DataType, Nullable, Position: X)
        var columnMatch = Regex.Match(columnLine, @"^([^(]+)\s*\(([^,]+),\s*([^,]+),\s*Position:\s*(\d+)\)");
        
        if (!columnMatch.Success)
        {
            return new ColumnModel();
        }

        var columnName = columnMatch.Groups[1].Value.Trim();
        var dataType = columnMatch.Groups[2].Value.Trim();
        var isNullable = columnMatch.Groups[3].Value.Trim();
        var position = columnMatch.Groups[4].Value.Trim();
        var foreignKeyInfo = fkInfo;

        var column = new ColumnModel
        {
            TableSchema = tableSchema,
            TableName = tableName,
            ColumnName = columnName,
            DataType = dataType,
            IsNullable = isNullable.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO",
            OrdinalPosition = position,
            ColumnDefault = string.Empty,
            CharacterMaximumLength = ExtractMaxLength(dataType),
            ForeignKeyName = string.Empty,
            ReferencedTableSchema = string.Empty,
            ReferencedTableName = string.Empty,
            ReferencedColumnName = string.Empty
        };

        // Parse foreign key if present
        if (!string.IsNullOrWhiteSpace(foreignKeyInfo))
        {
            var fkMatch = Regex.Match(foreignKeyInfo, @"^([^→]+)→\s*([^.]+)\.([^.]+)\.(.+)$");
            if (fkMatch.Success)
            {
                column.ForeignKeyName = fkMatch.Groups[1].Value.Trim();
                column.ReferencedTableSchema = fkMatch.Groups[2].Value.Trim();
                column.ReferencedTableName = fkMatch.Groups[3].Value.Trim();
                column.ReferencedColumnName = fkMatch.Groups[4].Value.Trim();
            }
        }

        return column;
    }

    private string ExtractMaxLength(string dataType)
    {
        var match = Regex.Match(dataType, @"\((\d+)\)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private void SaveCurrentTable(string schema, string name, string type, List<ColumnModel> columns)
    {
        var table = new TableModel
        {
            TableSchema = schema,
            TableName = name,
            TableType = type
        };

        _tables.Add(table);
        
        var key = $"{schema}.{name}";
        _tableColumns[key] = columns;
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
