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
		string schemaFileName = baseName.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
			? baseName
			: baseName + ".schema.md";
		string schemaFilePath = Path.Combine(Global.SchemasDirectory, schemaFileName);

		// overwrite if exists
		if (File.Exists(schemaFilePath))
		{
			File.Delete(schemaFilePath);
		}

		// Use new high-level Markdown writer API
		ISchemaWriter writer = _fileProvider.GetSchemaWriter(schemaFilePath);

        await writer.WriteConnectionStringAsync(connectionString);
        await writer.WriteTablesAsync(tables);
        await writer.WriteColumnsAsync(columns);
        await writer.WriteStoredProceduresAsync(sps);
		await writer.SaveAsync();

		writer.Dispose();

		// set newly dumped schema as active
		Global.SetActiveSchema(schemaFileName);

		_display.ShowInfo($"Dump database schema has been done. Active schema: {schemaFileName}");
    }
}
