using System.Text.Json;
using PRS.Database;
using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.FileHandle;

public class JsonSchemaReaderTests : IDisposable
{
    private readonly string _tempFilePath;
    private readonly JsonSchemaModel _testModel;

    public JsonSchemaReaderTests()
    {
        _tempFilePath = Path.GetTempFileName();
        _testModel = new JsonSchemaModel
        {
            ConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;",
            Tables = new List<JsonTableModel>
            {
                new JsonTableModel
                {
                    TableSchema = "dbo",
                    TableName = "Users",
                    TableType = "BASE TABLE",
                    Columns = new List<JsonColumnModel>
                    {
                        new JsonColumnModel { ColumnName = "UserId", DataType = "int", IsNullable = "NO", OrdinalPosition = "1", IsPrimaryKey = true },
                        new JsonColumnModel { ColumnName = "UserName", DataType = "nvarchar(100)", IsNullable = "NO", OrdinalPosition = "2" }
                    }
                },
                new JsonTableModel
                {
                    TableSchema = "dbo",
                    TableName = "Orders",
                    TableType = "BASE TABLE",
                    Columns = new List<JsonColumnModel>
                    {
                        new JsonColumnModel { ColumnName = "OrderId", DataType = "int", IsNullable = "NO", OrdinalPosition = "1", IsPrimaryKey = true },
                        new JsonColumnModel { ColumnName = "UserId", DataType = "int", IsNullable = "NO", OrdinalPosition = "2", ReferencedTableSchema = "dbo", ReferencedTableName = "Users", ReferencedColumnName = "UserId" }
                    }
                }
            },
            StoredProcedures = new List<string> { "sp_GetUserOrders", "sp_CreateOrder" }
        };
    }

    private void WriteJsonContent(JsonSchemaModel model)
    {
        string json = JsonSerializer.Serialize(model);
        File.WriteAllText(_tempFilePath, json);
    }

    [Fact]
    public async Task ReadConnectionStringAsync_ReturnsCorrectConnectionString()
    {
        // Arrange
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

        // Act
        var connectionString = await reader.ReadConnectionStringAsync();

        // Assert
        Assert.Equal("Server=localhost;Database=TestDB;Integrated Security=true;", connectionString);
    }

    [Fact]
    public async Task ReadTablesAsync_ReturnsAllTables()
    {
        // Arrange
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

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
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadAllColumnsAsync();

        // Assert
        Assert.Equal(4, columns.Count());
        Assert.Contains(columns, c => c.ColumnName == "UserId" && c.TableName == "Users");
        Assert.Contains(columns, c => c.ColumnName == "OrderId" && c.TableName == "Orders");
    }

    [Fact]
    public async Task ReadColumnsForTableAsync_ReturnsCorrectColumnsForTable()
    {
        // Arrange
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.ReadColumnsForTableAsync("Users");

        // Assert
        Assert.Equal(2, columns.Count());
        Assert.All(columns, c => Assert.Equal("Users", c.TableName));
    }

    [Fact]
    public async Task ReadStoredProceduresAsync_ReturnsAllStoredProcedures()
    {
        // Arrange
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

        // Act
        var procedures = await reader.ReadStoredProceduresAsync();

        // Assert
        Assert.Equal(2, procedures.Count());
        Assert.Contains(procedures, p => p == "sp_GetUserOrders");
        Assert.Contains(procedures, p => p == "sp_CreateOrder");
    }

    [Fact]
    public async Task FindTablesAsync_ReturnsMatchingTables()
    {
        // Arrange
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

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
        WriteJsonContent(_testModel);
        using var reader = new JsonSchemaReader(_tempFilePath);

        // Act
        var columns = await reader.FindColumnsAsync("Id");

        // Assert
        Assert.Equal(3, columns.Count()); // Users.UserId, Orders.OrderId, Orders.UserId
        Assert.Contains(columns, c => c.ColumnName == "UserId" && c.TableName == "Users");
        Assert.Contains(columns, c => c.ColumnName == "OrderId" && c.TableName == "Orders");
        Assert.Contains(columns, c => c.ColumnName == "UserId" && c.TableName == "Orders");
    }

    [Fact]
    public void Constructor_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentFile = Path.GetTempFileName();
        File.Delete(nonExistentFile);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new JsonSchemaReader(nonExistentFile));
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}
