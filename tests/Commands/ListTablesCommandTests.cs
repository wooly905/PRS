using Moq;
using PRS.Commands;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.Commands;

public class ListTablesCommandTests
{
    private readonly Mock<IDisplay> _mockDisplay = new();
    private readonly Mock<IFileProvider> _mockFileProvider = new();
    private readonly Mock<ISchemaReader> _mockSchemaReader = new();

    [Fact]
    public async Task RunAsync_ArgumentMismatch_ShowsError()
    {
        var command = new ListTablesCommand(_mockDisplay.Object, _mockFileProvider.Object);
        string[] args = ["prs", "lt", "extra_arg"];

        await command.RunAsync(args);

        _mockDisplay.Verify(d => d.ShowError("Argument mismatch"), Times.Once);
        _mockDisplay.Verify(d => d.ShowInfo(It.Is<string>(s => s.Contains("prs lt"))), Times.Once);
    }

    [Fact]
    public async Task RunAsync_DdlFormat_RejectsFormat()
    {
        var command = new ListTablesCommand(_mockDisplay.Object, _mockFileProvider.Object);
        string[] args = ["prs", "lt", "-f", "ddl"];

        await command.RunAsync(args);

        _mockDisplay.Verify(d => d.ShowError("The 'ddl' format is only supported by the 'sc' command."), Times.Once);
    }

    [Fact]
    public async Task RunAsync_SchemaDoesNotExist_ShowsError()
    {
        var originalAppData = Environment.GetEnvironmentVariable("APPDATA");
        var tempPath = Path.Combine(Path.GetTempPath(), "PRS.Tests", Guid.NewGuid().ToString());
        Environment.SetEnvironmentVariable("APPDATA", tempPath);

        try
        {
            var command = new ListTablesCommand(_mockDisplay.Object, _mockFileProvider.Object);
            string[] args = ["prs", "lt"];

            await command.RunAsync(args);

            _mockDisplay.Verify(d => d.ShowError("Schema doesn't exist locally. Please run dds command first."), Times.Once);
        }
        finally
        {
            Environment.SetEnvironmentVariable("APPDATA", originalAppData);
        }
    }

    [Fact]
    public async Task RunAsync_WithValidSchema_DisplaysAllTables()
    {
        var originalAppData = Environment.GetEnvironmentVariable("APPDATA");
        var tempPath = Path.Combine(Path.GetTempPath(), "PRS.Tests", Guid.NewGuid().ToString());
        Environment.SetEnvironmentVariable("APPDATA", tempPath);

        var schemaDir = Path.Combine(tempPath, ".prs", "schemas");
        Directory.CreateDirectory(schemaDir);
        var tempFile = Path.Combine(schemaDir, "schema.md");
        File.WriteAllText(tempFile, "# Schema");

        try
        {
            var tables = new List<TableModel>
            {
                new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" },
                new() { TableSchema = "dbo", TableName = "Orders", TableType = "BASE TABLE" }
            };

            _mockSchemaReader.Setup(r => r.ReadTablesAsync()).ReturnsAsync(tables);
            _mockFileProvider.Setup(p => p.GetSchemaReader(tempFile)).Returns(_mockSchemaReader.Object);

            var command = new ListTablesCommand(_mockDisplay.Object, _mockFileProvider.Object);
            string[] args = ["prs", "lt"];

            await command.RunAsync(args);

            _mockDisplay.Verify(d => d.DisplayTables(tables, OutputFormat.Table), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            Environment.SetEnvironmentVariable("APPDATA", originalAppData);
        }
    }
}
