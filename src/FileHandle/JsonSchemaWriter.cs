using System.Text.Json;
using System.Text.Json.Serialization;
using PRS.Database;

namespace PRS.FileHandle;

internal class JsonSchemaWriter : ISchemaWriter
{
    private readonly string _filePath;
    private string _connectionString = string.Empty;
    private readonly List<TableModel> _tables = new();
    private readonly List<ColumnModel> _columns = new();
    private readonly List<string> _storedProcedures = new();

    public JsonSchemaWriter(string filePath)
    {
        _filePath = filePath;
    }

    public Task WriteConnectionStringAsync(string connectionString)
    {
        _connectionString = connectionString;
        return Task.CompletedTask;
    }

    public Task WriteTablesAsync(IEnumerable<TableModel> tables)
    {
        _tables.Clear();
        _tables.AddRange(tables);
        return Task.CompletedTask;
    }

    public Task WriteColumnsAsync(IEnumerable<ColumnModel> columns)
    {
        _columns.Clear();
        _columns.AddRange(columns);
        return Task.CompletedTask;
    }

    public Task WriteStoredProceduresAsync(IEnumerable<string> procedureNames)
    {
        _storedProcedures.Clear();
        _storedProcedures.AddRange(procedureNames);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        var jsonModel = new JsonSchemaModel
        {
            ConnectionString = _connectionString,
            StoredProcedures = _storedProcedures.ToList()
        };

        var columnsByTable = _columns
            .GroupBy(c => (c.TableSchema, c.TableName))
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var t in _tables)
        {
            var tableKey = (t.TableSchema, t.TableName);
            var tableColumns = columnsByTable.TryGetValue(tableKey, out var cols)
                ? cols.Select(c => new JsonColumnModel
                  {
                      ColumnName = c.ColumnName,
                      OrdinalPosition = c.OrdinalPosition,
                      ColumnDefault = string.IsNullOrEmpty(c.ColumnDefault) ? null : c.ColumnDefault,
                      IsNullable = c.IsNullable,
                      DataType = c.DataType,
                      CharacterMaximumLength = string.IsNullOrEmpty(c.CharacterMaximumLength) ? null : c.CharacterMaximumLength,
                      IsPrimaryKey = c.IsPrimaryKey,
                      IsUnique = c.IsUnique,
                      IsIdentity = c.IsIdentity,
                      IdentitySeed = string.IsNullOrEmpty(c.IdentitySeed) ? null : c.IdentitySeed,
                      IdentityIncrement = string.IsNullOrEmpty(c.IdentityIncrement) ? null : c.IdentityIncrement,
                      ReferencedTableSchema = string.IsNullOrEmpty(c.ReferencedTableSchema) ? null : c.ReferencedTableSchema,
                      ReferencedTableName = string.IsNullOrEmpty(c.ReferencedTableName) ? null : c.ReferencedTableName,
                      ReferencedColumnName = string.IsNullOrEmpty(c.ReferencedColumnName) ? null : c.ReferencedColumnName
                  }).ToList()
                : new List<JsonColumnModel>();

            jsonModel.Tables.Add(new JsonTableModel
            {
                TableSchema = t.TableSchema,
                TableName = t.TableName,
                TableType = t.TableType,
                Columns = tableColumns
            });
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };
        string jsonContent = JsonSerializer.Serialize(jsonModel, options);

        // Ensure directory exists
        string directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(_filePath, jsonContent);
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
