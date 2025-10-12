using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.FileHandle;

public class XmlSchemaReaderTests : IDisposable
{
    private readonly string _testSchemaPath;

    public XmlSchemaReaderTests()
    {
        _testSchemaPath = TestFileHelper.CreateTestSchemaFile("test.schema.xml");
    }

    [Fact]
    public async Task ReadConnectionStringAsync_ReturnsConnectionString()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var connectionString = await reader.ReadConnectionStringAsync();

        // Assert
        Assert.NotNull(connectionString);
        Assert.Contains("TestDB", connectionString);
    }

    [Fact]
    public async Task ReadTablesAsync_ReturnsAllTables()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var tables = await reader.ReadTablesAsync();

        // Assert
        Assert.NotNull(tables);
        var tableList = tables.ToList();
        Assert.NotEmpty(tableList);
        Assert.Contains(tableList, t => t.TableName == "Users");
        Assert.Contains(tableList, t => t.TableName == "Orders");
        Assert.Contains(tableList, t => t.TableName == "Products");
        Assert.Contains(tableList, t => t.TableName == "OrderDetails");
        Assert.Contains(tableList, t => t.TableName == "UserRoles");
    }

    [Fact]
    public async Task ReadAllColumnsAsync_ReturnsAllColumns()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.ReadAllColumnsAsync();

        // Assert
        Assert.NotNull(columns);
        var columnList = columns.ToList();
        Assert.NotEmpty(columnList);

        // Verify some columns exist
        Assert.Contains(columnList, c => c.TableName == "Users" && c.ColumnName == "UserId");
        Assert.Contains(columnList, c => c.TableName == "Users" && c.ColumnName == "UserName");
        Assert.Contains(columnList, c => c.TableName == "Orders" && c.ColumnName == "OrderId");
    }

    [Fact]
    public async Task ReadAllColumnsAsync_IncludesForeignKeyInformation()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.ReadAllColumnsAsync();

        // Assert
        var orderUserIdColumn = columns.FirstOrDefault(c =>
            c.TableName == "Orders" && c.ColumnName == "UserId");

        Assert.NotNull(orderUserIdColumn);
        Assert.Equal("FK_Orders_Users", orderUserIdColumn.ForeignKeyName);
        Assert.Equal("Users", orderUserIdColumn.ReferencedTableName);
        Assert.Equal("UserId", orderUserIdColumn.ReferencedColumnName);
    }

    [Fact]
    public async Task ReadColumnsForTableAsync_WithExactMatch_ReturnsTableColumns()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("Users");

        // Assert
        var columnList = columns.ToList();
        Assert.NotEmpty(columnList);
        Assert.All(columnList, c => Assert.Equal("Users", c.TableName));
        Assert.Contains(columnList, c => c.ColumnName == "UserId");
        Assert.Contains(columnList, c => c.ColumnName == "UserName");
        Assert.Contains(columnList, c => c.ColumnName == "Email");
    }

    [Fact]
    public async Task ReadColumnsForTableAsync_WithCaseInsensitive_ReturnsTableColumns()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("users"); // lowercase

        // Assert
        var columnList = columns.ToList();
        Assert.NotEmpty(columnList);
    }

    [Fact]
    public async Task ReadColumnsForTableAsync_WithNonExistentTable_ReturnsEmpty()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("NonExistentTable");

        // Assert
        Assert.Empty(columns);
    }

    [Fact]
    public async Task ReadStoredProceduresAsync_ReturnsAllProcedures()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var procedures = await reader.ReadStoredProceduresAsync();

        // Assert
        var procedureList = procedures.ToList();
        Assert.NotEmpty(procedureList);
        Assert.Contains("sp_GetUserOrders", procedureList);
        Assert.Contains("sp_CreateOrder", procedureList);
        Assert.Contains("sp_UpdateOrderStatus", procedureList);
    }

    [Fact]
    public async Task FindTablesAsync_WithPartialName_ReturnsMatches()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var tables = await reader.FindTablesAsync("User");

        // Assert
        var tableList = tables.ToList();
        Assert.NotEmpty(tableList);
        Assert.Contains(tableList, t => t.TableName == "Users");
        Assert.Contains(tableList, t => t.TableName == "UserRoles");
        Assert.DoesNotContain(tableList, t => t.TableName == "Orders");
    }

    [Fact]
    public async Task FindTablesAsync_WithCaseInsensitive_ReturnsMatches()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var tables = await reader.FindTablesAsync("order"); // lowercase

        // Assert
        var tableList = tables.ToList();
        Assert.NotEmpty(tableList);
        Assert.Contains(tableList, t => t.TableName == "Orders");
        Assert.Contains(tableList, t => t.TableName == "OrderDetails");
    }

    [Fact]
    public async Task FindTablesAsync_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var tables = await reader.FindTablesAsync("NonExistent");

        // Assert
        Assert.Empty(tables);
    }

    [Fact]
    public async Task FindColumnsAsync_WithPartialName_ReturnsMatches()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.FindColumnsAsync("Id");

        // Assert
        var columnList = columns.ToList();
        Assert.NotEmpty(columnList);
        Assert.Contains(columnList, c => c.ColumnName == "UserId");
        Assert.Contains(columnList, c => c.ColumnName == "OrderId");
        Assert.Contains(columnList, c => c.ColumnName == "ProductId");
    }

    [Fact]
    public async Task FindColumnsAsync_WithCaseInsensitive_ReturnsMatches()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.FindColumnsAsync("name"); // lowercase

        // Assert
        var columnList = columns.ToList();
        Assert.NotEmpty(columnList);
        Assert.Contains(columnList, c => c.ColumnName == "UserName");
        Assert.Contains(columnList, c => c.ColumnName == "ProductName");
    }

    [Fact]
    public async Task FindColumnsAsync_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        using var reader = new XmlSchemaReader(_testSchemaPath);

        // Act
        var columns = await reader.FindColumnsAsync("NonExistentColumn");

        // Assert
        Assert.Empty(columns);
    }

    [Fact]
    public void Constructor_WithNonExistentFile_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<FileNotFoundException>(() => new XmlSchemaReader("nonexistent.xml"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

