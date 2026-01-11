using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class FindTableCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _testSchemaPath;

    public FindTableCommandTests()
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
    public async Task RunAsync_WithValidTableName_FindsMatchingTables()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft", "User" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Users"));
        Assert.True(_display.ContainsAnyMessage("UserRoles"));
        Assert.False(_display.ContainsAnyMessage("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithPartialTableName_FindsAllMatches()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft", "Order" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Orders"));
        Assert.True(_display.ContainsAnyMessage("OrderDetails"));
        Assert.False(_display.ContainsAnyMessage("Users"));
    }

    [Fact]
    public async Task RunAsync_WithCaseInsensitiveSearch_FindsTables()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft", "users" }; // lowercase

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Users"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentTableName_ShowsNothingFound()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft", "NonExistentTable" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsInfo("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft" }; // missing table name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithViewType_FindsViews()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft", "vw_" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("vw_UserOrders"));
    }

    [Fact]
    public async Task RunAsync_WithSingleCharacter_FindsMatches()
    {
        // Arrange
        var command = new FindTableCommand(_display, _fileProvider);
        var args = new[] { "ft", "P" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Products"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

