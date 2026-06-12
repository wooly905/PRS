using PRS.Display;

namespace PRS.Commands;

internal class RemoveSchemaCommand(IDisplay display) : ICommand
{
	private readonly IDisplay _display = display;

	public async Task RunAsync(string[] args)
	{
		await Task.Yield();

		if (args == null || args.Length != 2)
		{
			_display.ShowError("Argument mismatch");
			_display.ShowInfo("prs rm [schema name]");
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

		// if removing active schema, clear active pointer
		string active = Global.GetActiveSchemaName();
		if (!string.IsNullOrWhiteSpace(active) && string.Equals(active, target, StringComparison.OrdinalIgnoreCase))
		{
			Global.SetActiveSchema(null);
		}

		File.Delete(path);
		_display.ShowInfo($"Schema removed: {args[1]}");
	}
}


