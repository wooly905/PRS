namespace PRS.Database;

internal interface IDatabase
{
    Task<IEnumerable<TableModel>> GetTableModelsAsync(string connectionString);

    Task<IEnumerable<ColumnModel>> GetColumnModelsAsync(string connectionString);

    Task<IEnumerable<string>> GetStoredProcedureNamesAsync(string connectionString);
}
