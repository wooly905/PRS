using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class FindColumnCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // Parse optional output format: prs fc <column> [-f <format>]
        var (format, cleanArgs) = CommandHelper.ParseOutputFormat(args);

        if (CommandHelper.RejectDdlFormat(format, _display)) return;

        // verify args
        if (cleanArgs == null || cleanArgs.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs fc [column name] [-f table|json|text]");
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

        // Use new Markdown reader API with partial string search
        using ISchemaReader reader = _fileProvider.GetSchemaReader(Global.SchemaFilePath);
        IEnumerable<ColumnModel> models = await reader.FindColumnsAsync(cleanArgs[1]);

        if (models.Any())
        {
            CommandHelper.PrintColumns(models, _display, format);
        }
        else
        {
            _display.ShowInfo("Nothing found.");
        }
    }
}
