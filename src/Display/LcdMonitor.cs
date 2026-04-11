using PRS.Database;
using PRS.Formatting;
using Spectre.Console;

namespace PRS.Display;

internal class LcdMonitor : IDisplay
{
    public void DisplayColumns(IEnumerable<ColumnModel> models, OutputFormat format = OutputFormat.Table)
    {
        if (format != OutputFormat.Table)
        {
            Console.WriteLine(SchemaFormatter.FormatColumns(models, format));
            return;
        }

        Table t = new();

        t.AddColumn("[bold yellow]Table[/]");
        t.AddColumn("[bold yellow]Column[/]");
        t.AddColumn("[bold yellow]Pos[/]");
        t.AddColumn("[bold yellow]Default[/]");
        t.AddColumn("[bold yellow]Nullable[/]");
        t.AddColumn("[bold yellow]DataType[/]");
        t.AddColumn("[bold yellow]PK[/]");
        t.AddColumn("[bold yellow]Unique[/]");
        t.AddColumn("[bold yellow]Identity[/]");
        t.AddColumn("[bold yellow]FK[/]");
        t.AddColumn("[bold yellow]FK.Table[/]");
        t.AddColumn("[bold yellow]FK.Column[/]");

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
                     model.IsPrimaryKey ? "[bold green]YES[/]" : "NO",
                     model.IsUnique ? "[bold green]YES[/]" : "NO",
                     identityDisplay,
                     !string.IsNullOrEmpty(model.ReferencedTableName) ? "[bold green]YES[/]" : "NO",
                     model.ReferencedTableName ?? string.Empty,
                     model.ReferencedColumnName ?? string.Empty);
        }

        t.Border = TableBorder.Rounded;
        t.Caption = new TableTitle($"{models.Count()} records above");

        AnsiConsole.Write(t);
    }

    public void DisplayTableSchema(IEnumerable<ColumnModel> columns, string tableName, OutputFormat format = OutputFormat.Table)
    {
        if (format != OutputFormat.Table)
        {
            Console.WriteLine(SchemaFormatter.FormatTableSchema(columns, tableName, null, true, format));
            return;
        }

        // Table format: reuse the column table display
        DisplayColumns(columns, OutputFormat.Table);
    }

    public void DisplayTables(IEnumerable<TableModel> models, OutputFormat format = OutputFormat.Table)
    {
        if (format != OutputFormat.Table)
        {
            Console.WriteLine(SchemaFormatter.FormatTables(models, format));
            return;
        }

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

    public void DisplayStoredProcedures(IEnumerable<string> procedures, OutputFormat format = OutputFormat.Table)
    {
        if (format != OutputFormat.Table)
        {
            Console.WriteLine(SchemaFormatter.FormatStoredProcedures(procedures, format));
            return;
        }

        // Table format for stored procedures
        var list = procedures.ToList();

        Table t = new();
        t.AddColumn("[bold yellow]Name[/]");

        foreach (string proc in list)
        {
            t.AddRow(proc);
        }

        t.Border = TableBorder.Rounded;
        t.Caption = new TableTitle($"{list.Count} records above");

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
