using PRS.Display;

namespace PRS.Commands;

internal class ListSchemasCommand(IDisplay display) : ICommand
{
	private readonly IDisplay _display = display;

	public async Task RunAsync(string[] args)
	{
		await Task.Yield();

		if (!Directory.Exists(Global.SchemasDirectory))
		{
			_display.ShowInfo("No schemas found.");
			return;
		}

		string active = Global.GetActiveSchemaName();
		string[] files = Directory.GetFiles(Global.SchemasDirectory, "*", SearchOption.TopDirectoryOnly)
			.Where(f => f.EndsWith(".schema.json", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase))
			.ToArray();

		if (files == null || files.Length == 0)
		{
			_display.ShowInfo("No schemas found.");
			return;
		}

		_display.ShowInfo("Saved schemas:");
		foreach (string f in files)
		{
			string name = Path.GetFileName(f);
			string shortName = name;
			if (name.EndsWith(".schema.json", StringComparison.OrdinalIgnoreCase))
			{
				shortName = name.Substring(0, name.Length - ".schema.json".Length);
			}
			else if (name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase))
			{
				shortName = name.Substring(0, name.Length - ".schema.md".Length);
			}
			if (!string.IsNullOrWhiteSpace(active) && string.Equals(name, active, StringComparison.OrdinalIgnoreCase))
			{
				_display.ShowInfo($"* {shortName} (active)");
			}
			else
			{
				_display.ShowInfo($"  {shortName}");
			}
		}
	}
}


