using System.Text;
using System.Linq;
using PRS.Database;

namespace PRS.FileHandle;

/// <summary>
/// Writes database schema to Markdown format with key-value structure.
/// Structure: Tables organized by schema.table with columns listed underneath
/// </summary>
internal class MarkdownSchemaWriter : ISchemaWriter
{
    private readonly string _filePath;
    private readonly StringBuilder _content;
    private readonly List<TableModel> _tables;
    private readonly List<ColumnModel> _columns;
    private readonly List<string> _storedProcedures;
    private string _connectionString = string.Empty;

    public MarkdownSchemaWriter(string filePath)
    {
        _filePath = filePath;
        _content = new StringBuilder();
        _tables = new List<TableModel>();
        _columns = new List<ColumnModel>();
        _storedProcedures = new List<string>();
    }

    public Task WriteConnectionStringAsync(string connectionString)
    {
        _connectionString = connectionString ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task WriteTablesAsync(IEnumerable<TableModel> tables)
    {
        if (tables != null)
        {
            _tables.AddRange(tables);
        }
        return Task.CompletedTask;
    }

    public Task WriteColumnsAsync(IEnumerable<ColumnModel> columns)
    {
        if (columns != null)
        {
            _columns.AddRange(columns);
        }
        return Task.CompletedTask;
    }

    public Task WriteStoredProceduresAsync(IEnumerable<string> procedureNames)
    {
        if (procedureNames != null)
        {
            _storedProcedures.AddRange(procedureNames);
        }
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        await Task.Run(() =>
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Build markdown content
            BuildMarkdownContent();

            // Write to file
            File.WriteAllText(_filePath, _content.ToString(), Encoding.UTF8);
        });
    }

    private void BuildMarkdownContent()
    {
        _content.Clear();

        // Header
        _content.AppendLine("# Database Schema");
        _content.AppendLine();

        // Connection String
        _content.AppendLine("## Connection String");
        _content.AppendLine("```");
        _content.AppendLine(_connectionString);
        _content.AppendLine("```");
        _content.AppendLine();

        // Tables
        _content.AppendLine("## Tables");
        _content.AppendLine();

        // Group columns by table
        var columnsByTable = _columns
            .GroupBy(c => c.TableName)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => int.TryParse(c.OrdinalPosition, out var pos) ? pos : 999).ToList());

        // Write tables in the order they were provided
        foreach (var table in _tables)
        {
            var tableKey = table.TableName;
            var foundColumns = columnsByTable.TryGetValue(tableKey, out List<ColumnModel> value) ? value : [];
            WriteTable(table, foundColumns);
        }

        // Stored Procedures
        if (_storedProcedures.Any())
        {
            _content.AppendLine("## Stored Procedures");
            foreach (var procedure in _storedProcedures.OrderBy(p => p))
            {
                _content.AppendLine($"- {procedure}");
            }
            _content.AppendLine();
        }
    }

    private void WriteTable(TableModel table, List<ColumnModel> columns)
    {
        // Table header
        _content.AppendLine($"### {table.TableName}");
        
        // Table type
        _content.AppendLine($"- **Type**: {table.TableType}");
        
        // Columns
        if (columns.Any())
        {
            _content.AppendLine("- **Columns**:");
            foreach (var column in columns)
            {
                WriteColumn(column);
            }
        }
        
        _content.AppendLine();
    }

    private void WriteColumn(ColumnModel column)
    {
        var dataType = FormatDataType(column.DataType, column.CharacterMaximumLength);
        var nullable = column.IsNullable?.Equals("YES", StringComparison.OrdinalIgnoreCase) == true ? "NULL" : "NOT NULL";
        var position = column.OrdinalPosition ?? "0";
        
        var identity = column.IsIdentity ? $", Identity({column.IdentitySeed}, {column.IdentityIncrement})" : "";
        var pk = column.IsPrimaryKey ? ", PK" : "";
        var unique = column.IsUnique && !column.IsPrimaryKey ? ", Unique" : "";
        
        _content.Append($"  - {column.ColumnName} ({dataType}, {nullable}, Position: {position}{identity}{pk}{unique})");
        
        // Add foreign key information if present
        if (!string.IsNullOrWhiteSpace(column.ForeignKeyName) && 
            !string.IsNullOrWhiteSpace(column.ReferencedTableName) &&
            !string.IsNullOrWhiteSpace(column.ReferencedColumnName))
        {
            _content.AppendLine();
            _content.Append($"    - **FK**: {column.ForeignKeyName} → {column.ReferencedTableName}.{column.ReferencedColumnName}");
        }
        
        _content.AppendLine();
    }

    private string FormatDataType(string dataType, string maxLength)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return "unknown";
        }

        var baseType = dataType.ToLowerInvariant();
        
        // Add length for string types if specified
        if (!string.IsNullOrWhiteSpace(maxLength) && 
            (baseType.Contains("char") || baseType.Contains("varchar") || baseType.Contains("nvarchar")))
        {
            return $"{dataType}({maxLength})";
        }

        return dataType;
    }

    public void Dispose()
    {
        _content?.Clear();
    }
}
