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

    [Fact]
    public async Task RunAsync_DisplaysColumnsInAlphabeticalOrder()
    {
        // Arrange
        var command = new ShowAllColumnsCommand(_display, _fileProvider);
        var args = new[] { "sc", "Users" };

        // Act
        await command.RunAsync(args);

        // Assert
        // Get all column messages and extract column names
        var columnMessages = _display.InfoMessages
            .Where(m => m.StartsWith("Column:"))
            .ToList();

        Assert.NotEmpty(columnMessages);

        // Extract column names from messages
        var columnNames = columnMessages
            .Select(m => m.Split('.')[1].Split(' ')[0]) // Extract column name from "Column: Users.ColumnName (DataType)"
            .ToList();

        // Verify columns are in alphabetical order
        var expectedOrder = columnNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(expectedOrder, columnNames);
    }

    [Fact]
    public async Task RunAsync_DisplaysColumnsInAlphabeticalOrder_WithMixedCase()
    {
        // Arrange - Create a test schema with mixed case column names
        var testSchemaContent = @"# Database Schema

## Connection String
```
Server=localhost;Database=TestDB;Integrated Security=true;
```

## Tables

### TestTable
- **Type**: BASE TABLE
- **Columns**:
  - ZColumn (int, NOT NULL, Position: 1)
  - aColumn (nvarchar(100), NOT NULL, Position: 2)
  - BColumn (nvarchar(255), NULL, Position: 3)
  - cColumn (datetime, NOT NULL, Position: 4)
";

        var testSchemaPath = Path.Combine(TestFileHelper.GetTempPath(), ".prs", "schemas", "test_mixed_case.schema.md");
        Directory.CreateDirectory(Path.GetDirectoryName(testSchemaPath)!);
        await File.WriteAllTextAsync(testSchemaPath, testSchemaContent);

        // Set the active schema for this test
        var originalActiveSchema = Global.GetActiveSchemaName();
        Global.SetActiveSchema("test_mixed_case.schema.md");

        try
        {
            var command = new ShowAllColumnsCommand(_display, _fileProvider);
            var args = new[] { "sc", "TestTable" };

            // Act
            await command.RunAsync(args);

            // Assert
            var columnMessages = _display.InfoMessages
                .Where(m => m.StartsWith("Column:"))
                .ToList();

            Assert.NotEmpty(columnMessages);

            // Extract column names from messages
            var columnNames = columnMessages
                .Select(m => m.Split('.')[1].Split(' ')[0])
                .ToList();

            // Verify columns are in alphabetical order (case-insensitive)
            var expectedOrder = columnNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
            Assert.Equal(expectedOrder, columnNames);

            // Verify specific order: aColumn, BColumn, cColumn, ZColumn
            Assert.Equal("aColumn", columnNames[0]);
            Assert.Equal("BColumn", columnNames[1]);
            Assert.Equal("cColumn", columnNames[2]);
            Assert.Equal("ZColumn", columnNames[3]);
        }
        finally
        {
            // Restore original active schema
            if (!string.IsNullOrEmpty(originalActiveSchema))
            {
                Global.SetActiveSchema(originalActiveSchema);
            }
            else
            {
                // Clear the active schema if there wasn't one originally
                Global.SetActiveSchema(string.Empty);
            }
            
            if (File.Exists(testSchemaPath))
            {
                File.Delete(testSchemaPath);
            }
        }
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

