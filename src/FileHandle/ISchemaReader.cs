using PRS.Database;

namespace PRS.FileHandle;

/// <summary>
/// High-level interface for reading database schema from storage (Markdown format).
/// </summary>
public interface ISchemaReader : IDisposable
{
    /// <summary>
    /// Reads the connection string.
    /// </summary>
    Task<string> ReadConnectionStringAsync();

    /// <summary>
    /// Reads all tables.
    /// </summary>
    Task<IEnumerable<TableModel>> ReadTablesAsync();

    /// <summary>
    /// Reads all columns from all tables.
    /// </summary>
    Task<IEnumerable<ColumnModel>> ReadAllColumnsAsync();

    /// <summary>
    /// Reads columns for a specific table (exact match).
    /// </summary>
    Task<IEnumerable<ColumnModel>> ReadColumnsForTableAsync(string tableName);

    /// <summary>
    /// Reads all stored procedure names.
    /// </summary>
    Task<IEnumerable<string>> ReadStoredProceduresAsync();

    /// <summary>
    /// Finds tables by partial name match (case-insensitive).
    /// </summary>
    Task<IEnumerable<TableModel>> FindTablesAsync(string partialName);

    /// <summary>
    /// Finds columns by partial column name match (case-insensitive).
    /// </summary>
    Task<IEnumerable<ColumnModel>> FindColumnsAsync(string partialName);
}

