using PRS.Database;
using Spectre.Console;

namespace PRS.Display;

internal class LcdMonitor : IDisplay
{
    public void DisplayColumns(IEnumerable<ColumnModel> models)
    {
        Table t = new();

        t.AddColumn("[bold yellow]Table[/]");
        t.AddColumn("[bold yellow]Column[/]");
        t.AddColumn("[bold yellow]Pos[/]");
        t.AddColumn("[bold yellow]Default[/]");
        t.AddColumn("[bold yellow]Nullable[/]");
        t.AddColumn("[bold yellow]DataType[/]");
        t.AddColumn("[bold yellow]CharMaxLength[/]");
        t.AddColumn("[bold yellow]PK[/]");
        t.AddColumn("[bold yellow]Unique[/]");
        t.AddColumn("[bold yellow]Identity[/]");
        t.AddColumn("[bold yellow]ForeignKeyName[/]");
        t.AddColumn("[bold yellow]RefTableName[/]");
        t.AddColumn("[bold yellow]RefColumnName[/]");

        foreach (ColumnModel model in models)
        {
            string identityDisplay = model.IsIdentity
                ? $"YES ({model.IdentitySeed}, {model.IdentityIncrement})"
                : "NO";

            t.AddRow(model.TableName ?? string.Empty,
                     model.ColumnName ?? string.Empty,
                     model.OrdinalPosition ?? string.Empty,
                     model.ColumnDefault ?? string.Empty,
                     model.IsNullable ?? string.Empty,
                     model.DataType ?? string.Empty,
                     model.CharacterMaximumLength ?? string.Empty,
                     model.IsPrimaryKey ? "[bold green]YES[/]" : "NO",
                     model.IsUnique ? "[bold green]YES[/]" : "NO",
                     identityDisplay,
                     model.ForeignKeyName ?? string.Empty,
                     model.ReferencedTableName ?? string.Empty,
                     model.ReferencedColumnName ?? string.Empty);
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
            t.AddRow(model.TableName,
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
