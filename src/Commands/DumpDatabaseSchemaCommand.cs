using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class DumpDatabaseSchemaCommand(IDisplay display, IDatabase database, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IDatabase _database = database;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // verify default directory. create the directory.
        if (Directory.Exists(Global.SchemaFileDirectory))
        {
            if (File.Exists(Path.Combine(Global.SchemaFileDirectory, Global.SchemaFileName)))
            {
                File.Delete(Path.Combine(Global.SchemaFileDirectory, Global.SchemaFileName));
            }
        }
        else
        {
            Directory.CreateDirectory(Global.SchemaFileDirectory);
        }

        // run database tool to get data
        string connectionString = await CommandHelper.GetConnectionStringAsync(_display, _fileProvider);

        if (connectionString == null)
        {
            _display.ShowError("Connection string is not found.");
            return;
        }

        _display.ShowInfo("Starting to dump schema...");

        IEnumerable<TableModel> tables = await _database.GetTableModelsAsync(connectionString);
        IEnumerable<ColumnModel> columns = await _database.GetColumnModelsAsync(connectionString);
        IEnumerable<string> sps = await _database.GetStoredProcedureNamesAsync(connectionString);

        // insert connection string and all data to file
        IFileWriter writer = _fileProvider.GetFileWriter(Path.Combine(Global.SchemaFileDirectory, Global.SchemaFileName));

        await WriteConnectionStringAsync(writer, connectionString);
        await WriteTablesAsync(writer, tables);
        await WriteColumnsAsync(writer, columns);
        await WriteStoredProceduresAsync(writer, sps);

        writer.Dispose();

        _display.ShowInfo("Dump database schema has been done.");
    }

    private async Task WriteConnectionStringAsync(IFileWriter writer, string connectionString)
    {
        await writer.WriteLineAsync(Global.ConnectionStringSectionName);
        await writer.WriteLineAsync(connectionString);
    }

    private async Task WriteTablesAsync(IFileWriter writer, IEnumerable<TableModel> tables)
    {
        await writer.WriteLineAsync(Global.TableSectionName);

        foreach (TableModel m in tables)
        {
            string s = $"{m.TableSchema},{m.TableName},{m.TableType}";
            await writer.WriteLineAsync(s);
        }
    }

    private async Task WriteColumnsAsync(IFileWriter writer, IEnumerable<ColumnModel> columns)
    {
        await writer.WriteLineAsync(Global.ColumnSectionName);

        foreach (ColumnModel m in columns)
        {
            string s = $"{m.TableSchema},{m.TableName},{m.ColumnName},{m.OrdinalPosition},{m.ColumnDefault},{m.IsNullable},{m.DataType},{m.CharacterMaximumLength}";
            await writer.WriteLineAsync(s);
        }
    }

    private async Task WriteStoredProceduresAsync(IFileWriter writer, IEnumerable<string> sps)
    {
        await writer.WriteLineAsync(Global.StoredProcedureSectionName);

        foreach (string m in sps)
        {
            await writer.WriteLineAsync(m);
        }
    }
}
