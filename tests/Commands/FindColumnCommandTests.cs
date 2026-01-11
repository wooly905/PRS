using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class FindColumnCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _testSchemaPath;

    public FindColumnCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        
        // Set APPDATA to test temp path
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
        
        // Create test schema file in the location Global expects
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/schema.md");
        _testSchemaPath = Path.Combine(TestFileHelper.GetTempPath(), ".prs", "schemas", "schema.md");
    }

    [Fact]
    public async Task RunAsync_WithValidColumnName_FindsMatchingColumns()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc", "UserId" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("Users")); // table name
        Assert.False(_display.ContainsAnyMessage("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithPartialColumnName_FindsAllMatches()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc", "Id" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("OrderId"));
        Assert.True(_display.ContainsAnyMessage("ProductId"));
    }

    [Fact]
    public async Task RunAsync_WithCaseInsensitiveSearch_FindsColumns()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc", "userid" }; // lowercase

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentColumnName_ShowsNothingFound()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc", "NonExistentColumn" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsInfo("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc" }; // missing column name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_FindsColumnsAcrossMultipleTables()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc", "Name" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserName"));
        Assert.True(_display.ContainsAnyMessage("ProductName"));
        Assert.True(_display.ContainsAnyMessage("RoleName"));
    }

    [Fact]
    public async Task RunAsync_WithCommonColumnName_FindsMultipleOccurrences()
    {
        // Arrange
        var command = new FindColumnCommand(_display, _fileProvider);
        var args = new[] { "fc", "Date" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("CreatedDate"));
        Assert.True(_display.ContainsAnyMessage("OrderDate"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

