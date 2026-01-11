using PRS.Database;
using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.FileHandle;

public class MarkdownSchemaReaderTests : IDisposable
{
    private readonly string _tempFilePath;
    private readonly string _testContent;

    public MarkdownSchemaReaderTests()
    {
        _tempFilePath = Path.GetTempFileName();
        _testContent = @"# Database Schema

## Connection String
```
Server=localhost;Database=TestDB;Integrated Security=true;
```

## Tables

### Users
- **Type**: BASE TABLE
- **Columns**:
  - UserId (int, NOT NULL, Position: 1)
  - UserName (nvarchar(100), NOT NULL, Position: 2)
  - Email (nvarchar(255), NULL, Position: 3)
  - CreatedDate (datetime, NOT NULL, Position: 4)

### Orders
- **Type**: BASE TABLE
- **Columns**:
  - OrderId (int, NOT NULL, Position: 1)
  - UserId (int, NOT NULL, Position: 2)
    - **FK**: FK_Orders_Users → Users.UserId
  - OrderDate (datetime, NOT NULL, Position: 3)
  - TotalAmount (decimal, NULL, Position: 4)

## Stored Procedures
- sp_GetUserOrders
- sp_CreateOrder
- sp_UpdateOrderStatus
";
    }

    [Fact]
    public async Task ReadConnectionStringAsync_ReturnsCorrectConnectionString()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var connectionString = await reader.ReadConnectionStringAsync();

        // Assert
        Assert.Equal("Server=localhost;Database=TestDB;Integrated Security=true;", connectionString);
    }

    [Fact]
    public async Task ReadTablesAsync_ReturnsAllTables()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var tables = await reader.ReadTablesAsync();

        // Assert
        Assert.Equal(2, tables.Count());
        Assert.Contains(tables, t => t.TableName == "Users");
        Assert.Contains(tables, t => t.TableName == "Orders");
    }

    [Fact]
    public async Task ReadAllColumnsAsync_ReturnsAllColumns()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadAllColumnsAsync();

        // Assert
        Assert.Equal(8, columns.Count()); // 4 columns in Users + 4 columns in Orders
        Assert.Contains(columns, c => c.ColumnName == "UserId" && c.TableName == "Users");
        Assert.Contains(columns, c => c.ColumnName == "UserName" && c.TableName == "Users");
        Assert.Contains(columns, c => c.ColumnName == "OrderId" && c.TableName == "Orders");
    }

    [Fact]
    public async Task ReadColumnsForTableAsync_ReturnsCorrectColumnsForTable()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("Users");

        // Assert
        Assert.Equal(4, columns.Count());
        Assert.All(columns, c => Assert.Equal("Users", c.TableName));
    }

    [Fact]
    public async Task ReadStoredProceduresAsync_ReturnsAllStoredProcedures()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var procedures = await reader.ReadStoredProceduresAsync();

        // Assert
        Assert.Equal(3, procedures.Count());
        Assert.Contains(procedures, p => p == "sp_GetUserOrders");
        Assert.Contains(procedures, p => p == "sp_CreateOrder");
        Assert.Contains(procedures, p => p == "sp_UpdateOrderStatus");
    }

    [Fact]
    public async Task FindTablesAsync_ReturnsMatchingTables()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var tables = await reader.FindTablesAsync("User");

        // Assert
        Assert.Single(tables);
        Assert.Equal("Users", tables.First().TableName);
    }

    [Fact]
    public async Task FindColumnsAsync_ReturnsMatchingColumns()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.FindColumnsAsync("Id");

        // Assert
        Assert.Equal(3, columns.Count()); // UserId (from Users), OrderId, and UserId (from Orders)
        Assert.Contains(columns, c => c.ColumnName == "UserId");
        Assert.Contains(columns, c => c.ColumnName == "OrderId");
        // Note: There are two UserId columns (one from Users table, one from Orders table as FK)
    }

    [Fact]
    public async Task ReadColumnsAsync_ParsesForeignKeyCorrectly()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, _testContent);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("Orders");
        var userColumn = columns.First(c => c.ColumnName == "UserId");

        // Assert
        Assert.Equal("FK_Orders_Users", userColumn.ForeignKeyName);
        Assert.Equal("Users", userColumn.ReferencedTableName);
        Assert.Equal("UserId", userColumn.ReferencedColumnName);
    }

    [Fact]
    public async Task ReadColumnsAsync_ParsesIdentityCorrectly()
    {
        // Arrange
        var content = @"# Database Schema
## Tables
### IdentityTable
- **Type**: BASE TABLE
- **Columns**:
  - Id (int, NOT NULL, Position: 1, Identity(100, 10))";

        File.WriteAllText(_tempFilePath, content);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("IdentityTable");
        var column = columns.First();

        // Assert
        Assert.True(column.IsIdentity);
        Assert.Equal("100", column.IdentitySeed);
        Assert.Equal("10", column.IdentityIncrement);
    }

    [Fact]
    public async Task ReadColumnsAsync_ParsesPKAndUniqueCorrectly()
    {
        // Arrange
        var content = @"# Database Schema
## Tables
### TestTable
- **Type**: BASE TABLE
- **Columns**:
  - PkCol (int, NOT NULL, Position: 1, PK)
  - UniqueCol (nvarchar(50), NULL, Position: 2, Unique)
  - BothCol (int, NOT NULL, Position: 3, PK, Unique)";

        File.WriteAllText(_tempFilePath, content);
        using var reader = new MarkdownSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("TestTable");
        var pkCol = columns.First(c => c.ColumnName == "PkCol");
        var uniqueCol = columns.First(c => c.ColumnName == "UniqueCol");
        var bothCol = columns.First(c => c.ColumnName == "BothCol");

        // Assert
        Assert.True(pkCol.IsPrimaryKey);
        Assert.True(pkCol.IsUnique); // PK should imply Unique in our logic

        Assert.False(uniqueCol.IsPrimaryKey);
        Assert.True(uniqueCol.IsUnique);

        Assert.True(bothCol.IsPrimaryKey);
        Assert.True(bothCol.IsUnique);
    }

    [Fact]
    public void Constructor_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentFile = Path.GetTempFileName();
        File.Delete(nonExistentFile);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new MarkdownSchemaReader(nonExistentFile));
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}
