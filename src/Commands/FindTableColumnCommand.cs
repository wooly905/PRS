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

        // read schema file line by line and search table and column 
        IFileReader reader = _fileProvider.GetFileReader(Global.SchemaFilePath);
        bool found = false;

        while (true)
        {
            string line = await reader.ReadLineAsync();

            if (line == null)
            {
                // end of file
                break;
            }

            if (string.Equals(line, Global.ColumnSectionName))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            _display.ShowInfo("Nothing found");
            return;
        }

        // find targets 
        List<ColumnModel> models = new();

        while (true)
        {
            string line = await reader.ReadLineAsync();

            if (line == null)
            {
                // end of file
                break;
            }

            if (line.StartsWith("["))
            {
                // reach other section
                break;
            }

			string[] splits = line.Split(new char[] { ',' });

			if (splits[1]?.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0
				&& splits[2]?.IndexOf(args[2], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ColumnModel m = new()
                {
                    TableSchema = splits[0],
                    TableName = splits[1],
                    ColumnName = splits[2],
                    OrdinalPosition = splits[3],
                    ColumnDefault = splits[4],
                    IsNullable = splits[5],
                    DataType = splits[6],
					CharacterMaximumLength = splits[7],
					ForeignKeyName = splits.Length > 8 ? splits[8] : null,
					ReferencedTableSchema = splits.Length > 9 ? splits[9] : null,
					ReferencedTableName = splits.Length > 10 ? splits[10] : null,
					ReferencedColumnName = splits.Length > 11 ? splits[11] : null
                };
                models.Add(m);
            }
        }

        reader.Dispose();

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
