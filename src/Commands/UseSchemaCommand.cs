using PRS.Display;

namespace PRS.Commands;

internal class UseSchemaCommand(IDisplay display) : ICommand
{
	private readonly IDisplay _display = display;

	public async Task RunAsync(string[] args)
	{
		await Task.Yield();

		if (args == null || args.Length != 2)
		{
			_display.ShowError("Argument mismatch");
			_display.ShowInfo("prs use [schema name]");
			return;
		}

		string name = Global.SafeFileName(args[1]);
		string target = name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
			? name
			: name + ".schema.md";
		string path = Path.Combine(Global.SchemasDirectory, target);

		if (!File.Exists(path))
		{
			_display.ShowError("Schema not found.");
			return;
		}

		Global.SetActiveSchema(target);
		_display.ShowInfo($"Active schema switched to: {Path.GetFileNameWithoutExtension(target).Replace(".schema", string.Empty)}");
	}
}


