using PRS.Database;
using Spectre.Console;

namespace PRS.Display;

internal class LcdMonitor : IDisplay
{
    public void DisplayColumns(IEnumerable<ColumnModel> models)
    {
        List<string> properties = typeof(ColumnModel).GetProperties().Select(p => p.Name).ToList();

        Table t = new();

        foreach (string name in properties)
        {
            t.AddColumn($"[bold yellow]{name}[/]");
        }

        foreach (ColumnModel model in models)
        {
            t.AddRow(model.TableSchema,
                     model.TableName,
                     model.ColumnName,
                     model.OrdinalPosition,
                     model.ColumnDefault,
                     model.IsNullable,
                     model.DataType,
                     model.CharacterMaximumLength);
        }

        t.Border = TableBorder.Rounded;
        t.Caption = new TableTitle($"{models.Count()} records above");

        AnsiConsole.Write(t);
    }

    public void DisplayTables(IEnumerable<TableModel> models)
    {
        List<string> properties = typeof(TableModel).GetProperties().Select(p => p.Name).ToList();

        Table t = new();

        foreach (string name in properties)
        {
            t.AddColumn($"[bold yellow]{name}[/]");
        }

        foreach (TableModel model in models)
        {
            t.AddRow(model.TableSchema,
                     model.TableName,
                     model.TableType);
        }

        t.Border = TableBorder.Rounded;

        AnsiConsole.Write(t);
    }

    public void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void ShowInfo(string message)
    {
        Console.WriteLine(message);
    }
}
