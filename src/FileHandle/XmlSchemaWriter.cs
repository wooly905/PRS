using System.Xml.Linq;
using PRS.Database;

namespace PRS.FileHandle;

/// <summary>
/// Writes database schema to XML format with nested structure.
/// Structure: Databases -> Database -> Tables -> Table -> Columns -> Column
/// </summary>
internal class XmlSchemaWriter : ISchemaWriter
{
    private readonly string _filePath;
    private readonly XDocument _xdoc;
    private readonly XElement _databaseElement;
    private readonly XElement _tablesElement;
    private readonly XElement _storedProceduresElement;
    private readonly Dictionary<string, XElement> _tableElements;

    public XmlSchemaWriter(string filePath)
    {
        _filePath = filePath;
        _tableElements = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

        // Create XML structure
        _databaseElement = new XElement("Database");
        _tablesElement = new XElement("Tables");
        _storedProceduresElement = new XElement("StoredProcedures");

        _databaseElement.Add(_tablesElement);
        _databaseElement.Add(_storedProceduresElement);

        _xdoc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Databases", _databaseElement)
        );
    }

    public Task WriteConnectionStringAsync(string connectionString)
    {
        // Add connection string as first element in Database
        var csElement = new XElement("ConnectionString", connectionString ?? string.Empty);
        _databaseElement.AddFirst(csElement);
        return Task.CompletedTask;
    }

    public Task WriteTablesAsync(IEnumerable<TableModel> tables)
    {
        if (tables == null) return Task.CompletedTask;

        foreach (var table in tables)
        {
            var tableElement = new XElement("Table",
                new XElement("Schema", table.TableSchema ?? string.Empty),
                new XElement("Name", table.TableName ?? string.Empty),
                new XElement("Type", table.TableType ?? string.Empty),
                new XElement("Columns") // Empty columns element, will be populated later
            );

            _tablesElement.Add(tableElement);

            // Store reference for later when adding columns
            string key = $"{table.TableSchema}.{table.TableName}";
            _tableElements[key] = tableElement;
        }

        return Task.CompletedTask;
    }

    public Task WriteColumnsAsync(IEnumerable<ColumnModel> columns)
    {
        if (columns == null) return Task.CompletedTask;

        foreach (var column in columns)
        {
            // Find the corresponding table element
            string key = $"{column.TableSchema}.{column.TableName}";
            if (!_tableElements.TryGetValue(key, out var tableElement))
            {
                // Table not found, skip this column
                continue;
            }

            // Get the Columns container element
            var columnsElement = tableElement.Element("Columns");
            if (columnsElement == null)
            {
                columnsElement = new XElement("Columns");
                tableElement.Add(columnsElement);
            }

            // Create column element
            var columnElement = new XElement("Column",
                new XElement("Name", column.ColumnName ?? string.Empty),
                new XElement("OrdinalPosition", column.OrdinalPosition ?? string.Empty),
                new XElement("ColumnDefault", column.ColumnDefault ?? string.Empty),
                new XElement("IsNullable", column.IsNullable ?? string.Empty),
                new XElement("DataType", column.DataType ?? string.Empty),
                new XElement("CharacterMaximumLength", column.CharacterMaximumLength ?? string.Empty),
                new XElement("ForeignKey",
                    new XElement("Name", column.ForeignKeyName ?? string.Empty),
                    new XElement("ReferencedTableSchema", column.ReferencedTableSchema ?? string.Empty),
                    new XElement("ReferencedTableName", column.ReferencedTableName ?? string.Empty),
                    new XElement("ReferencedColumnName", column.ReferencedColumnName ?? string.Empty)
                )
            );

            columnsElement.Add(columnElement);
        }

        return Task.CompletedTask;
    }

    public Task WriteStoredProceduresAsync(IEnumerable<string> procedureNames)
    {
        if (procedureNames == null) return Task.CompletedTask;

        foreach (var name in procedureNames)
        {
            var procElement = new XElement("Procedure",
                new XElement("Name", name ?? string.Empty)
            );
            _storedProceduresElement.Add(procElement);
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

            // Save with proper formatting
            _xdoc.Save(_filePath, SaveOptions.None);
        });
    }

    public void Dispose()
    {
        // XML document will be garbage collected
    }
}

