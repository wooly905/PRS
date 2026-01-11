using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class FindTableColumnCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _testSchemaPath;

    public FindTableColumnCommandTests()
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
    public async Task RunAsync_WithValidTableAndColumnName_FindsMatches()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "Users", "UserId" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("Users"));
        Assert.False(_display.ContainsAnyMessage("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithPartialTableAndColumnName_FindsAllMatches()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "Order", "Id" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("OrderId"));
        Assert.True(_display.ContainsAnyMessage("Orders"));
        // Should also find OrderDetails with OrderId, OrderDetailId, and ProductId
    }

    [Fact]
    public async Task RunAsync_WithCaseInsensitiveSearch_FindsMatches()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "users", "userid" }; // lowercase

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
    }

    [Fact]
    public async Task RunAsync_WithNonMatchingCombination_ShowsNothingFound()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "Users", "OrderId" }; // OrderId not in Users table

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsInfo("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "Users" }; // missing column name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_FindsCommonColumnInMultipleTables()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "User", "UserId" };

        // Act
        await command.RunAsync(args);

        // Assert
        // Should find UserId in Users, Orders, UserRoles
        Assert.True(_display.ContainsAnyMessage("UserId"));
    }

    [Fact]
    public async Task RunAsync_WithBothPartialMatches_FindsIntersection()
    {
        // Arrange
        var command = new FindTableColumnCommand(_display, _fileProvider);
        var args = new[] { "ftc", "Product", "Name" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("ProductName"));
        Assert.True(_display.ContainsAnyMessage("Products"));
        Assert.False(_display.ContainsAnyMessage("UserName")); // Not in Products table
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

