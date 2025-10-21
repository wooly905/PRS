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
		string[] files = Directory.GetFiles(Global.SchemasDirectory, "*.schema.md", SearchOption.TopDirectoryOnly);

		if (files == null || files.Length == 0)
		{
			_display.ShowInfo("No schemas found.");
			return;
		}

		_display.ShowInfo("Saved schemas:");
		foreach (string f in files)
		{
			string name = Path.GetFileName(f);
			string shortName = name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
				? name.Substring(0, name.Length - ".schema.md".Length)
				: name;
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


