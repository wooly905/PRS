using PRS.Database;
using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.FileHandle;

public class MarkdownSchemaWriterTests : IDisposable
{
    private readonly string _tempFilePath;

    public MarkdownSchemaWriterTests()
    {
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public async Task SaveAsync_WritesCorrectMarkdownFormat()
    {
        // Arrange
        var tables = new List<TableModel>
        {
            new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" },
            new() { TableSchema = "dbo", TableName = "Orders", TableType = "BASE TABLE" }
        };

        var columns = new List<ColumnModel>
        {
            new() { TableSchema = "dbo", TableName = "Users", ColumnName = "UserId", DataType = "int", IsNullable = "NO", OrdinalPosition = "1" },
            new() { TableSchema = "dbo", TableName = "Users", ColumnName = "UserName", DataType = "nvarchar", CharacterMaximumLength = "100", IsNullable = "NO", OrdinalPosition = "2" },
            new() { TableSchema = "dbo", TableName = "Orders", ColumnName = "OrderId", DataType = "int", IsNullable = "NO", OrdinalPosition = "1" },
            new() { TableSchema = "dbo", TableName = "Orders", ColumnName = "UserId", DataType = "int", IsNullable = "NO", OrdinalPosition = "2", 
                    ForeignKeyName = "FK_Orders_Users", ReferencedTableSchema = "dbo", ReferencedTableName = "Users", ReferencedColumnName = "UserId" }
        };

        var procedures = new List<string> { "sp_GetUserOrders", "sp_CreateOrder" };

        using var writer = new MarkdownSchemaWriter(_tempFilePath);

        // Act
        await writer.WriteConnectionStringAsync("Server=localhost;Database=TestDB;");
        await writer.WriteTablesAsync(tables);
        await writer.WriteColumnsAsync(columns);
        await writer.WriteStoredProceduresAsync(procedures);
        await writer.SaveAsync();

        // Assert
        var content = await File.ReadAllTextAsync(_tempFilePath);
        Assert.Contains("# Database Schema", content);
        Assert.Contains("Server=localhost;Database=TestDB;", content);
        Assert.Contains("### dbo.Users", content);
        Assert.Contains("### dbo.Orders", content);
        Assert.Contains("UserId (int, NOT NULL, Position: 1)", content);
        Assert.Contains("UserName (nvarchar(100), NOT NULL, Position: 2)", content);
        Assert.Contains("**FK**: FK_Orders_Users → dbo.Users.UserId", content);
        Assert.Contains("## Stored Procedures", content);
        Assert.Contains("- sp_GetUserOrders", content);
        Assert.Contains("- sp_CreateOrder", content);
    }

    [Fact]
    public async Task SaveAsync_HandlesEmptyData()
    {
        // Arrange
        using var writer = new MarkdownSchemaWriter(_tempFilePath);

        // Act
        await writer.SaveAsync();

        // Assert
        var content = await File.ReadAllTextAsync(_tempFilePath);
        Assert.Contains("# Database Schema", content);
        Assert.Contains("## Connection String", content);
        Assert.Contains("## Tables", content);
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "prs_test", Guid.NewGuid().ToString());
        var filePath = Path.Combine(tempDir, "test.md");
        
        using var writer = new MarkdownSchemaWriter(filePath);

        // Act
        await writer.SaveAsync();

        // Assert
        Assert.True(Directory.Exists(tempDir));
        Assert.True(File.Exists(filePath));

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task WriteColumnsAsync_OrdersColumnsByPosition()
    {
        // Arrange
        var table = new TableModel { TableSchema = "dbo", TableName = "Test", TableType = "BASE TABLE" };
        var columns = new List<ColumnModel>
        {
            new() { TableSchema = "dbo", TableName = "Test", ColumnName = "Col3", DataType = "int", IsNullable = "NO", OrdinalPosition = "3" },
            new() { TableSchema = "dbo", TableName = "Test", ColumnName = "Col1", DataType = "int", IsNullable = "NO", OrdinalPosition = "1" },
            new() { TableSchema = "dbo", TableName = "Test", ColumnName = "Col2", DataType = "int", IsNullable = "NO", OrdinalPosition = "2" }
        };

        using var writer = new MarkdownSchemaWriter(_tempFilePath);

        // Act
        await writer.WriteTablesAsync(new[] { table });
        await writer.WriteColumnsAsync(columns);
        await writer.SaveAsync();

        // Assert
        var content = await File.ReadAllTextAsync(_tempFilePath);
        var lines = content.Split('\n');
        var columnLines = lines.Where(l => l.Contains("Col")).ToArray();
        
        Assert.Contains("Col1", columnLines[1]);
        Assert.Contains("Col2", columnLines[2]);
        Assert.Contains("Col3", columnLines[3]);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}
