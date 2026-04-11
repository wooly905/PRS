using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class ShowAllColumnsCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // Parse optional output format: prs sc <table> [-f <format>]
        var (format, cleanArgs) = CommandHelper.ParseOutputFormat(args);

        // verify args
        if (cleanArgs == null || cleanArgs.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs sc [table name] [-f table|ddl|json|text]");
            return;
        }

        // verify schema file exists. if not, show no schema file error and ask to run dump command.
        if (!File.Exists(Global.SchemaFilePath))
        {
            _display.ShowError("Schema doesn't exist locally. Please run dds command first.");
            return;
        }

		// show active schema in use
		_display.ShowInfo($"Using schema: {Path.GetFileName(Global.SchemaFilePath)}");

        // Use new Markdown reader API with exact table name match
        using ISchemaReader reader = _fileProvider.GetSchemaReader(Global.SchemaFilePath);
        IEnumerable<ColumnModel> models = await reader.ReadColumnsForTableAsync(cleanArgs[1]);

        if (models.Any())
        {
            // Sort columns by name alphabetically before displaying
            var sortedModels = models.OrderBy(m => m.ColumnName, StringComparer.OrdinalIgnoreCase);
            CommandHelper.PrintTableSchema(sortedModels, cleanArgs[1], _display, format);
        }
        else
        {
            _display.ShowInfo("Nothing found.");
        }
    }

}
