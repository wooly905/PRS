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
		// verify args
		if (args == null || args.Length != 2)
		{
			_display.ShowError("Argument mismatch");
			_display.ShowInfo("prs dds [schema name]");
			return;
		}

		// ensure directories exist
		if (!Directory.Exists(Global.SchemaFileDirectory))
		{
			Directory.CreateDirectory(Global.SchemaFileDirectory);
		}
		if (!Directory.Exists(Global.SchemasDirectory))
		{
			Directory.CreateDirectory(Global.SchemasDirectory);
		}

        // run database tool to get data
		string connectionString = await CommandHelper.GetConnectionStringAsync(_display, _fileProvider);

		if (string.IsNullOrWhiteSpace(connectionString))
        {
            _display.ShowError("Connection string is not found.");
            return;
        }

        _display.ShowInfo("Starting to dump schema...");

        IEnumerable<TableModel> tables = await _database.GetTableModelsAsync(connectionString);
        IEnumerable<ColumnModel> columns = await _database.GetColumnModelsAsync(connectionString);
        IEnumerable<string> sps = await _database.GetStoredProcedureNamesAsync(connectionString);

		// build schema file name from user-provided schema name and write all data
		string baseName = Global.SafeFileName(args[1]);
		string schemaFileName = baseName.EndsWith(".schema.txt", StringComparison.OrdinalIgnoreCase)
			? baseName
			: baseName + ".schema.txt";
		string schemaFilePath = Path.Combine(Global.SchemasDirectory, schemaFileName);

		// overwrite if exists
		if (File.Exists(schemaFilePath))
		{
			File.Delete(schemaFilePath);
		}
		IFileWriter writer = _fileProvider.GetFileWriter(schemaFilePath);

        await WriteConnectionStringAsync(writer, connectionString);
        await WriteTablesAsync(writer, tables);
        await WriteColumnsAsync(writer, columns);
        await WriteStoredProceduresAsync(writer, sps);

		writer.Dispose();

		// set newly dumped schema as active
		Global.SetActiveSchema(schemaFileName);

		_display.ShowInfo($"Dump database schema has been done. Active schema: {schemaFileName}");
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
			string s = $"{m.TableSchema},{m.TableName},{m.ColumnName},{m.OrdinalPosition},{m.ColumnDefault},{m.IsNullable},{m.DataType},{m.CharacterMaximumLength},{m.ForeignKeyName},{m.ReferencedTableSchema},{m.ReferencedTableName},{m.ReferencedColumnName}";
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
