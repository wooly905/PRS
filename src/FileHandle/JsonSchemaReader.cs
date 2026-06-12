using System.Text.Json;
using PRS.Database;

namespace PRS.FileHandle;

internal class JsonSchemaReader : ISchemaReader
{
    private readonly string _filePath;
    private readonly string _connectionString;
    private readonly List<TableModel> _tables;
    private readonly List<ColumnModel> _columns;
    private readonly List<string> _storedProcedures;

    public JsonSchemaReader(string filePath)
    {
        _filePath = filePath;

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Schema file not found: {filePath}");
        }

        string jsonContent = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var jsonModel = JsonSerializer.Deserialize<JsonSchemaModel>(jsonContent, options) ?? new JsonSchemaModel();

        _connectionString = jsonModel.ConnectionString ?? string.Empty;
        _storedProcedures = jsonModel.StoredProcedures ?? new List<string>();

        _tables = new List<TableModel>();
        _columns = new List<ColumnModel>();

        if (jsonModel.Tables != null)
        {
            foreach (var t in jsonModel.Tables)
            {
                _tables.Add(new TableModel
                {
                    TableSchema = t.TableSchema,
                    TableName = t.TableName,
                    TableType = t.TableType
                });

                if (t.Columns != null)
                {
                    foreach (var c in t.Columns)
                    {
                        _columns.Add(new ColumnModel
                        {
                            TableSchema = t.TableSchema,
                            TableName = t.TableName,
                            ColumnName = c.ColumnName,
                            OrdinalPosition = c.OrdinalPosition,
                            ColumnDefault = c.ColumnDefault ?? string.Empty,
                            IsNullable = c.IsNullable,
                            DataType = c.DataType,
                            CharacterMaximumLength = c.CharacterMaximumLength ?? string.Empty,
                            IsPrimaryKey = c.IsPrimaryKey,
                            IsUnique = c.IsUnique,
                            IsIdentity = c.IsIdentity,
                            IdentitySeed = c.IdentitySeed ?? string.Empty,
                            IdentityIncrement = c.IdentityIncrement ?? string.Empty,
                            ForeignKeyName = string.Empty,
                            ReferencedTableSchema = c.ReferencedTableSchema ?? string.Empty,
                            ReferencedTableName = c.ReferencedTableName ?? string.Empty,
                            ReferencedColumnName = c.ReferencedColumnName ?? string.Empty
                        });
                    }
                }
            }
        }
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
        return Task.FromResult<IEnumerable<ColumnModel>>(_columns);
    }

    public Task<IEnumerable<ColumnModel>> ReadColumnsForTableAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        var columns = _columns
            .Where(c => string.Equals(c.TableName, tableName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IEnumerable<ColumnModel>>(columns);
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

        var matchingTables = _tables
            .Where(t => t.TableName.Contains(partialName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IEnumerable<TableModel>>(matchingTables);
    }

    public Task<IEnumerable<ColumnModel>> FindColumnsAsync(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        var matchingColumns = _columns
            .Where(c => c.ColumnName.Contains(partialName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IEnumerable<ColumnModel>>(matchingColumns);
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
