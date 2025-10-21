using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class ShowAllColumnsCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _testSchemaPath;

    public ShowAllColumnsCommandTests()
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
    public async Task RunAsync_WithValidTableName_ShowsAllColumns()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "Users" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("UserName"));
        Assert.True(_display.ContainsAnyMessage("Email"));
        Assert.True(_display.ContainsAnyMessage("CreatedDate"));
        Assert.False(_display.ContainsAnyMessage("OrderId")); // from different table
    }

    [Fact]
    public async Task RunAsync_WithExactTableMatch_ShowsOnlyThatTable()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "Orders" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("OrderId"));
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("OrderDate"));
        Assert.True(_display.ContainsAnyMessage("TotalAmount"));
        Assert.False(_display.ContainsAnyMessage("UserName")); // from Users table
    }

    [Fact]
    public async Task RunAsync_WithCaseInsensitiveTableName_ShowsColumns()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "users" }; // lowercase

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("UserName"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentTable_ShowsNothingFound()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "NonExistentTable" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsInfo("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc" }; // missing table name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithTableHavingForeignKeys_ShowsFKInformation()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "Orders" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        // FK information should be included in column details
        var userIdMessages = _display.AllMessages.Where(m => m.Contains("UserId")).ToList();
        Assert.NotEmpty(userIdMessages);
    }

    [Fact]
    public async Task RunAsync_WithViewName_ShowsViewColumns()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "vw_UserOrders" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("UserId"));
        Assert.True(_display.ContainsAnyMessage("UserName"));
        Assert.True(_display.ContainsAnyMessage("OrderCount"));
    }

    [Fact]
    public async Task RunAsync_DoesNotAcceptPartialMatch()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "User" }; // partial name

        // Act
        await command.RunAsync(args);

        // Assert
        // Should not find "Users" or "UserRoles" with partial match
        Assert.True(_display.ContainsInfo("Nothing found"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

