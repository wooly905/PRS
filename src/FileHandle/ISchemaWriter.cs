using PRS.Database;

namespace PRS.FileHandle;

/// <summary>
/// High-level interface for writing database schema to storage (Markdown format).
/// </summary>
public interface ISchemaWriter : IDisposable
{
    /// <summary>
    /// Writes the connection string.
    /// </summary>
    Task WriteConnectionStringAsync(string connectionString);

    /// <summary>
    /// Writes all tables.
    /// </summary>
    Task WriteTablesAsync(IEnumerable<TableModel> tables);

    /// <summary>
    /// Writes all columns (will be organized under their respective tables).
    /// </summary>
    Task WriteColumnsAsync(IEnumerable<ColumnModel> columns);

    /// <summary>
    /// Writes all stored procedure names.
    /// </summary>
    Task WriteStoredProceduresAsync(IEnumerable<string> procedureNames);

    /// <summary>
    /// Saves the Markdown document to disk.
    /// </summary>
    Task SaveAsync();
}

