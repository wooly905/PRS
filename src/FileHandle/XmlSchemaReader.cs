using System.Xml.Linq;
using PRS.Database;

namespace PRS.FileHandle;

/// <summary>
/// Reads database schema from XML format with nested structure.
/// Supports partial string search for tables and columns.
/// </summary>
internal class XmlSchemaReader : ISchemaReader
{
    private readonly string _filePath;
    private readonly XDocument _xdoc;
    private readonly XElement _databaseElement;

    public XmlSchemaReader(string filePath)
    {
        _filePath = filePath;
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Schema file not found: {filePath}");
        }

        _xdoc = XDocument.Load(filePath);
        
        // Get the first Database element (support for single database per file for now)
        _databaseElement = _xdoc.Root?.Element("Database");
        
        if (_databaseElement == null)
        {
            throw new InvalidOperationException($"Invalid schema file format: {filePath}");
        }
    }

    public Task<string> ReadConnectionStringAsync()
    {
        var connectionString = _databaseElement.Element("ConnectionString")?.Value ?? string.Empty;
        return Task.FromResult(connectionString);
    }

    public Task<IEnumerable<TableModel>> ReadTablesAsync()
    {
        var tables = _databaseElement
            .Element("Tables")
            ?.Elements("Table")
            .Select(t => new TableModel
            {
                TableSchema = t.Element("Schema")?.Value ?? string.Empty,
                TableName = t.Element("Name")?.Value ?? string.Empty,
                TableType = t.Element("Type")?.Value ?? string.Empty
            })
            .ToList() ?? new List<TableModel>();

        return Task.FromResult<IEnumerable<TableModel>>(tables);
    }

    public Task<IEnumerable<ColumnModel>> ReadAllColumnsAsync()
    {
        var columns = new List<ColumnModel>();

        var tables = _databaseElement.Element("Tables")?.Elements("Table");
        if (tables == null) return Task.FromResult<IEnumerable<ColumnModel>>(columns);

        foreach (var table in tables)
        {
            var tableSchema = table.Element("Schema")?.Value ?? string.Empty;
            var tableName = table.Element("Name")?.Value ?? string.Empty;

            var tableColumns = table
                .Element("Columns")
                ?.Elements("Column")
                .Select(c => CreateColumnModel(c, tableSchema, tableName))
                .ToList();

            if (tableColumns != null)
            {
                columns.AddRange(tableColumns);
            }
        }

        return Task.FromResult<IEnumerable<ColumnModel>>(columns);
    }

    public Task<IEnumerable<ColumnModel>> ReadColumnsForTableAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        var tables = _databaseElement.Element("Tables")?.Elements("Table");
        if (tables == null) return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());

        // Find the table by exact name match (case-insensitive)
        var table = tables.FirstOrDefault(t => 
            string.Equals(t.Element("Name")?.Value, tableName, StringComparison.OrdinalIgnoreCase));

        if (table == null)
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        var tableSchema = table.Element("Schema")?.Value ?? string.Empty;
        var tableNameValue = table.Element("Name")?.Value ?? string.Empty;

        var columns = table
            .Element("Columns")
            ?.Elements("Column")
            .Select(c => CreateColumnModel(c, tableSchema, tableNameValue))
            .ToList() ?? new List<ColumnModel>();

        return Task.FromResult<IEnumerable<ColumnModel>>(columns);
    }

    public Task<IEnumerable<string>> ReadStoredProceduresAsync()
    {
        var procedures = _databaseElement
            .Element("StoredProcedures")
            ?.Elements("Procedure")
            .Select(p => p.Element("Name")?.Value ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();

        return Task.FromResult<IEnumerable<string>>(procedures);
    }

    public Task<IEnumerable<TableModel>> FindTablesAsync(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
        {
            return Task.FromResult<IEnumerable<TableModel>>(new List<TableModel>());
        }

        var tables = _databaseElement
            .Element("Tables")
            ?.Elements("Table")
            .Where(t =>
            {
                var tableName = t.Element("Name")?.Value;
                return tableName != null && 
                       tableName.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0;
            })
            .Select(t => new TableModel
            {
                TableSchema = t.Element("Schema")?.Value ?? string.Empty,
                TableName = t.Element("Name")?.Value ?? string.Empty,
                TableType = t.Element("Type")?.Value ?? string.Empty
            })
            .ToList() ?? new List<TableModel>();

        return Task.FromResult<IEnumerable<TableModel>>(tables);
    }

    public Task<IEnumerable<ColumnModel>> FindColumnsAsync(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
        {
            return Task.FromResult<IEnumerable<ColumnModel>>(new List<ColumnModel>());
        }

        var columns = new List<ColumnModel>();

        var tables = _databaseElement.Element("Tables")?.Elements("Table");
        if (tables == null) return Task.FromResult<IEnumerable<ColumnModel>>(columns);

        foreach (var table in tables)
        {
            var tableSchema = table.Element("Schema")?.Value ?? string.Empty;
            var tableName = table.Element("Name")?.Value ?? string.Empty;

            var matchingColumns = table
                .Element("Columns")
                ?.Elements("Column")
                .Where(c =>
                {
                    var columnName = c.Element("Name")?.Value;
                    return columnName != null && 
                           columnName.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .Select(c => CreateColumnModel(c, tableSchema, tableName))
                .ToList();

            if (matchingColumns != null)
            {
                columns.AddRange(matchingColumns);
            }
        }

        return Task.FromResult<IEnumerable<ColumnModel>>(columns);
    }

    private static ColumnModel CreateColumnModel(XElement columnElement, string tableSchema, string tableName)
    {
        var fkElement = columnElement.Element("ForeignKey");

        return new ColumnModel
        {
            TableSchema = tableSchema,
            TableName = tableName,
            ColumnName = columnElement.Element("Name")?.Value ?? string.Empty,
            OrdinalPosition = columnElement.Element("OrdinalPosition")?.Value ?? string.Empty,
            ColumnDefault = columnElement.Element("ColumnDefault")?.Value ?? string.Empty,
            IsNullable = columnElement.Element("IsNullable")?.Value ?? string.Empty,
            DataType = columnElement.Element("DataType")?.Value ?? string.Empty,
            CharacterMaximumLength = columnElement.Element("CharacterMaximumLength")?.Value ?? string.Empty,
            ForeignKeyName = fkElement?.Element("Name")?.Value ?? string.Empty,
            ReferencedTableSchema = fkElement?.Element("ReferencedTableSchema")?.Value ?? string.Empty,
            ReferencedTableName = fkElement?.Element("ReferencedTableName")?.Value ?? string.Empty,
            ReferencedColumnName = fkElement?.Element("ReferencedColumnName")?.Value ?? string.Empty
        };
    }

    public void Dispose()
    {
        // XDocument will be garbage collected
    }
}

