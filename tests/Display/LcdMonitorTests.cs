using PRS.Database;
using PRS.Display;
using Spectre.Console;
using Xunit;

namespace PRS.Tests.Display;

public class LcdMonitorTests
{
    [Fact]
    public void DisplayTables_TableFormat_IncludesAllPropertiesExceptSchema()
    {
        // Arrange
        using var sw = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(sw)
        });
        var monitor = new LcdMonitor(console);
        var tables = new List<TableModel>
        {
            new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" },
            new() { TableSchema = "sales", TableName = "Orders", TableType = "VIEW" }
        };

        // Act
        monitor.DisplayTables(tables, OutputFormat.Table);

        // Assert
        var output = sw.ToString();
        Assert.DoesNotContain("dbo", output);
        Assert.DoesNotContain("sales", output);
        Assert.Contains("Users", output);
        Assert.Contains("BASE TABLE", output);
        Assert.Contains("Orders", output);
        Assert.Contains("VIEW", output);
    }

    [Fact]
    public void DisplayTables_TableFormat_NullSchemaDoesNotThrow()
    {
        // Arrange
        using var sw = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(sw)
        });
        var monitor = new LcdMonitor(console);
        var tables = new List<TableModel>
        {
            new() { TableSchema = null, TableName = "Users", TableType = "BASE TABLE" }
        };

        // Act
        var ex = Record.Exception(() => monitor.DisplayTables(tables, OutputFormat.Table));

        // Assert
        Assert.Null(ex);
        var output = sw.ToString();
        Assert.Contains("Users", output);
        Assert.Contains("BASE TABLE", output);
    }
}
