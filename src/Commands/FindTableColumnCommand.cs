using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class FindTableColumnCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // verify args
        if (args == null || args.Length != 3)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs ftc [table name] [column name]");
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

        // Use new Markdown reader API with partial string search for both table and column
        using ISchemaReader reader = _fileProvider.GetSchemaReader(Global.SchemaFilePath);
        IEnumerable<ColumnModel> allColumns = await reader.ReadAllColumnsAsync();
        
        // Filter by both table name and column name (partial match)
        var models = allColumns.Where(c =>
            c.TableName?.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0 &&
            c.ColumnName?.IndexOf(args[2], StringComparison.OrdinalIgnoreCase) >= 0
        ).ToList();

        if (models.Count > 0)
        {
            CommandHelper.PrintColumns(models, _display);
        }
        else
        {
            _display.ShowInfo("Nothing found.");
        }
    }
}
