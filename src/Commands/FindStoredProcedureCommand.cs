using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class FindStoredProcedureCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // verify args
        if (args == null || args.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs fsp [stored procedure name]");
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
        IEnumerable<string> allProcedures = await reader.ReadStoredProceduresAsync();
        
        // Filter by partial name match
        var models = allProcedures.Where(sp =>
            sp?.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0
        ).ToList();

        if (models.Count > 0)
        {
            CommandHelper.PrintModel(models, _display);
        }
        else
        {
            _display.ShowInfo("Nothing found.");
        }
    }
}
