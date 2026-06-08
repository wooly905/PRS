using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class ListTablesCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        var (format, cleanArgs) = CommandHelper.ParseOutputFormat(args);

        if (CommandHelper.RejectDdlFormat(format, _display)) return;

        if (cleanArgs == null || cleanArgs.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs lt [-f table|json|text]");
            return;
        }

        if (!File.Exists(Global.SchemaFilePath))
        {
            _display.ShowError("Schema doesn't exist locally. Please run dds command first.");
            return;
        }

        _display.ShowInfo($"Using schema: {Path.GetFileName(Global.SchemaFilePath)}");

        using ISchemaReader reader = _fileProvider.GetSchemaReader(Global.SchemaFilePath);
        IEnumerable<TableModel> models = await reader.ReadTablesAsync();

        if (models != null && models.Any())
        {
            CommandHelper.PrintTables(models, _display, format);
        }
        else
        {
            _display.ShowInfo("Nothing found.");
        }
    }
}
