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
		string jsonTarget = name.EndsWith(".schema.json", StringComparison.OrdinalIgnoreCase) ? name : name + ".schema.json";
		string mdTarget = name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase) ? name : name + ".schema.md";

		string target = jsonTarget;
		string path = Path.Combine(Global.SchemasDirectory, jsonTarget);

		if (!File.Exists(path))
		{
			string mdPath = Path.Combine(Global.SchemasDirectory, mdTarget);
			if (File.Exists(mdPath))
			{
				target = mdTarget;
				path = mdPath;
			}
			else
			{
				_display.ShowError("Schema not found.");
				return;
			}
		}

		Global.SetActiveSchema(target);
		_display.ShowInfo($"Active schema switched to: {Path.GetFileNameWithoutExtension(target).Replace(".schema", string.Empty)}");
	}
}


