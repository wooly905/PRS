using PRS.Database;
using PRS.FileHandle;

namespace PRS.McpServer.Services;

internal class SchemaService
{
    private readonly IFileProvider _fileProvider;
    private readonly string _schemaFilePath;

    public SchemaService(IFileProvider fileProvider, string? schemaFilePath = null)
    {
        _fileProvider = fileProvider;
        _schemaFilePath = schemaFilePath ?? Global.SchemaFilePath;
    }

    public async Task<IEnumerable<TableModel>> FindTablesAsync(string keyword)
    {
        if (!File.Exists(_schemaFilePath))
        {
            return Enumerable.Empty<TableModel>();
        }

        using var reader = _fileProvider.GetSchemaReader(_schemaFilePath);
        return await reader.FindTablesAsync(keyword);
    }

    public async Task<IEnumerable<ColumnModel>> FindColumnsAsync(string keyword)
    {
        if (!File.Exists(_schemaFilePath))
        {
            return Enumerable.Empty<ColumnModel>();
        }

        using var reader = _fileProvider.GetSchemaReader(_schemaFilePath);
        return await reader.FindColumnsAsync(keyword);
    }

    public async Task<IEnumerable<string>> FindStoredProceduresAsync(string keyword)
    {
        if (!File.Exists(_schemaFilePath))
        {
            return Enumerable.Empty<string>();
        }

        using var reader = _fileProvider.GetSchemaReader(_schemaFilePath);
        var allProcedures = await reader.ReadStoredProceduresAsync();
        
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return allProcedures;
        }

        return allProcedures.Where(p => 
            p.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public async Task<IEnumerable<ColumnModel>> GetTableDetailsAsync(string tableName, string? schema = null)
    {
        if (!File.Exists(_schemaFilePath))
        {
            return Enumerable.Empty<ColumnModel>();
        }

        using var reader = _fileProvider.GetSchemaReader(_schemaFilePath);
        
        // If schema is provided, try schema.table format
        if (!string.IsNullOrWhiteSpace(schema))
        {
            var fullTableName = $"{schema}.{tableName}";
            var columns = await reader.ReadColumnsForTableAsync(fullTableName);
            if (columns.Any())
            {
                return columns;
            }
        }

        // Try just table name
        return await reader.ReadColumnsForTableAsync(tableName);
    }

    public async Task<SchemaListResult> ListSchemasAsync()
    {
        var result = new SchemaListResult
        {
            Schemas = new List<SchemaInfo>(),
            ActiveSchema = null
        };

        if (!Directory.Exists(Global.SchemasDirectory))
        {
            return result;
        }

        string active = Global.GetActiveSchemaName();
        string[] files = Directory.GetFiles(Global.SchemasDirectory, "*.schema.md", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string shortName = name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - ".schema.md".Length)
                : name;

            bool isActive = !string.IsNullOrWhiteSpace(active) && 
                          string.Equals(name, active, StringComparison.OrdinalIgnoreCase);

            result.Schemas.Add(new SchemaInfo
            {
                Name = shortName,
                FileName = name,
                IsActive = isActive
            });

            if (isActive)
            {
                result.ActiveSchema = shortName;
            }
        }

        return result;
    }

    public async Task<SwitchSchemaResult> SwitchSchemaAsync(string schemaName)
    {
        await Task.Yield();

        string safeName = Global.SafeFileName(schemaName);
        string target = safeName.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
            ? safeName
            : safeName + ".schema.md";
        string path = Path.Combine(Global.SchemasDirectory, target);

        if (!File.Exists(path))
        {
            return new SwitchSchemaResult
            {
                Success = false,
                Message = $"Schema '{schemaName}' not found.",
                ActiveSchema = Global.GetActiveSchemaName()
            };
        }

        Global.SetActiveSchema(target);
        string shortName = target.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
            ? target.Substring(0, target.Length - ".schema.md".Length)
            : target;

        return new SwitchSchemaResult
        {
            Success = true,
            Message = $"Active schema switched to: {shortName}",
            ActiveSchema = shortName
        };
    }
}

internal class SchemaListResult
{
    public List<SchemaInfo> Schemas { get; set; } = new();
    public string? ActiveSchema { get; set; }
}

internal class SchemaInfo
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

internal class SwitchSchemaResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ActiveSchema { get; set; }
}

