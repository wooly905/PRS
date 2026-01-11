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
		string target = name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
			? name
			: name + ".schema.md";
		string path = Path.Combine(Global.SchemasDirectory, target);

		if (!File.Exists(path))
		{
			_display.ShowError("Schema not found.");
			return;
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


