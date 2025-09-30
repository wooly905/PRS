using PRS.Database;
using PRS.Display;
using PRS.FileHandle;
using Terminal.Gui;
using static System.Net.Mime.MediaTypeNames;

namespace PRS.Commands;

internal class ErdCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        if (args == null || args.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs erd [table name]");
            return;
        }

        string targetTable = args[1];

        if (!File.Exists(Global.SchemaFilePath))
        {
            _display.ShowError("Schema doesn't exist locally. Please run dds command first.");
            return;
        }

        _display.ShowInfo($"Using schema: {Path.GetFileName(Global.SchemaFilePath)}");

        List<ColumnModel> allColumns = await ReadAllColumnsAsync();

        if (allColumns.Count == 0)
        {
            _display.ShowInfo("No column metadata found in schema file.");
            return;
        }

        // Build relationships
        var outgoing = allColumns
            .Where(c => string.Equals(c.TableName, targetTable, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(c.ReferencedTableName))
            .Select(c => new
            {
                FromSchema = c.TableSchema,
                FromTable = c.TableName,
                ToSchema = c.ReferencedTableSchema,
                ToTable = c.ReferencedTableName
            })
            .Distinct()
            .ToList();

        var incoming = allColumns
            .Where(c => !string.IsNullOrWhiteSpace(c.ReferencedTableName)
                        && string.Equals(c.ReferencedTableName, targetTable, StringComparison.OrdinalIgnoreCase))
            .Select(c => new
            {
                FromSchema = c.TableSchema,
                FromTable = c.TableName,
                ToSchema = c.ReferencedTableSchema,
                ToTable = c.ReferencedTableName
            })
            .Distinct()
            .ToList();

        if (outgoing.Count == 0 && incoming.Count == 0)
        {
            _display.ShowInfo("No foreign key relationships found for the specified table.");
            return;
        }

        string ascii = BuildAsciiBoxes(targetTable, allColumns);

        // Show via Terminal.Gui
        Terminal.Gui.Application.Init();
        try
        {
            var win = new Window()
            {
                Title = $"ERD: {targetTable}",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var textView = new TextView()
            {
                ReadOnly = true,
                WordWrap = false,
                Text = ascii,
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            win.Add(textView);

            // Exit on Esc or Ctrl+Q
            win.KeyDown += (sender, key) =>
            {
                if (key == Key.Esc)
                {
                    Terminal.Gui.Application.RequestStop();
                }
            };

            Terminal.Gui.Application.Run(win);
        }
        finally
        {
            Terminal.Gui.Application.Shutdown();
        }
    }

    private async Task<List<ColumnModel>> ReadAllColumnsAsync()
    {
        IFileReader reader = _fileProvider.GetFileReader(Global.SchemaFilePath);
        List<ColumnModel> columns = new();
        bool inColumnSection = false;

        try
        {
            while (true)
            {
                string line = await reader.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                if (!inColumnSection)
                {
                    if (string.Equals(line, Global.ColumnSectionName))
                    {
                        inColumnSection = true;
                    }
                    continue;
                }

                if (line.StartsWith("["))
                {
                    // next section reached
                    break;
                }

                string[] splits = line.Split(new char[] { ',' });
                if (splits.Length < 3)
                {
                    continue;
                }

                ColumnModel m = new()
                {
                    TableSchema = SafeGet(splits, 0),
                    TableName = SafeGet(splits, 1),
                    ColumnName = SafeGet(splits, 2),
                    OrdinalPosition = SafeGet(splits, 3),
                    ColumnDefault = SafeGet(splits, 4),
                    IsNullable = SafeGet(splits, 5),
                    DataType = SafeGet(splits, 6),
                    CharacterMaximumLength = SafeGet(splits, 7),
                    ForeignKeyName = SafeGet(splits, 8),
                    ReferencedTableSchema = SafeGet(splits, 9),
                    ReferencedTableName = SafeGet(splits, 10),
                    ReferencedColumnName = SafeGet(splits, 11)
                };
                columns.Add(m);
            }
        }
        finally
        {
            reader.Dispose();
        }

        return columns;
    }

    private static string SafeGet(string[] arr, int index)
    {
        return index >= 0 && index < arr.Length ? arr[index] : null;
    }

    private static string BuildAsciiBoxes(string targetTable, List<ColumnModel> allColumns)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"ERD for [{targetTable}]");
        sb.AppendLine();

        // Group outgoing relationships: targetTable -> ReferencedTable
        var outgoingGroups = allColumns
            .Where(c => string.Equals(c.TableName, targetTable, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(c.ReferencedTableName))
            .GroupBy(c => new { c.ReferencedTableSchema, c.ReferencedTableName })
            .OrderBy(g => g.Key.ReferencedTableName)
            .ToList();

        if (outgoingGroups.Count > 0)
        {
            sb.AppendLine("References:");
            foreach (var g in outgoingGroups)
            {
                string leftTitle = targetTable; // referencing side
                string rightTitle = g.Key.ReferencedTableName ?? string.Empty; // referenced side
                var colNames = g.Select(x => x.ColumnName).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
                string midLabel = colNames.Count == 0 ? "FK" : string.Join(", ", colNames);

                var lines = BuildRelationshipBlock(leftTitle, rightTitle, midLabel, manyOnLeft: true);
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
                sb.AppendLine();
            }
        }

        // Incoming relationships: other tables referencing targetTable
        var incomingGroups = allColumns
            .Where(c => !string.IsNullOrWhiteSpace(c.ReferencedTableName)
                        && string.Equals(c.ReferencedTableName, targetTable, StringComparison.OrdinalIgnoreCase))
            .GroupBy(c => new { c.TableSchema, c.TableName })
            .OrderBy(g => g.Key.TableName)
            .ToList();

        if (incomingGroups.Count > 0)
        {
            sb.AppendLine("Referenced By:");
            foreach (var g in incomingGroups)
            {
                string leftTitle = g.Key.TableName; // referencing side
                string rightTitle = targetTable;    // referenced side
                var colNames = g.Select(x => x.ColumnName).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
                string midLabel = colNames.Count == 0 ? "FK" : string.Join(", ", colNames);

                var lines = BuildRelationshipBlock(leftTitle, rightTitle, midLabel, manyOnLeft: true);
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine("Press Esc to exit.");
        return sb.ToString();
    }

    private static List<string> BuildRelationshipBlock(string leftTitle, string rightTitle, string midLabel, bool manyOnLeft)
    {
        int leftInner = Math.Max(10, leftTitle?.Length ?? 0);
        int rightInner = Math.Max(20, rightTitle?.Length ?? 0);
        string starSide = manyOnLeft ? "*" : "1";
        string oneSide = manyOnLeft ? "1" : "*";
        string starLineContent = $" {starSide}    {midLabel}   {oneSide} ";
        int gapWidth = Math.Max(22, starLineContent.Length);

        string LeftTop() => "+" + new string('-', leftInner) + "+";
        string LeftBottom() => LeftTop();
        string RightTop() => "+" + new string('-', rightInner) + "+";
        string RightBottom() => RightTop();
        string LeftInterior(string content)
            => "|" + (content ?? string.Empty).PadRight(leftInner) + "|";
        string RightInterior(string content)
            => "|" + (content ?? string.Empty).PadRight(rightInner) + "|";

        string Gap(string content)
        {
            if (string.IsNullOrEmpty(content)) return new string(' ', gapWidth);
            if (content.Length >= gapWidth) return content;
            int padLeft = (gapWidth - content.Length) / 2;
            int padRight = gapWidth - content.Length - padLeft;
            return new string(' ', padLeft) + content + new string(' ', padRight);
        }

        string top = LeftTop() + Gap(string.Empty) + RightTop();
        string row1 = LeftInterior(string.Empty) + Gap(starLineContent) + RightInterior(string.Empty);
        string row2 = LeftInterior(leftTitle) + new string('-', gapWidth) + RightInterior(rightTitle);
        string row3 = LeftInterior(string.Empty) + Gap(string.Empty) + RightInterior(string.Empty);
        string bottom = LeftBottom() + Gap(string.Empty) + RightBottom();

        return new List<string> { top, row1, row2, row3, bottom };
    }

    private static string Qualified(string schema, string table)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            return table ?? string.Empty;
        }
        return $"{schema}.{table}";
    }
}
