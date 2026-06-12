using System.Text.Json;
using PRS.Database;
using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.FileHandle;

public class JsonSchemaWriterTests : IDisposable
{
    private readonly string _tempFilePath;

    public JsonSchemaWriterTests()
    {
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public async Task WriteAndSaveAsync_CreatesCorrectJsonFile()
    {
        // Arrange
        using var writer = new JsonSchemaWriter(_tempFilePath);
        var tables = new List<TableModel>
        {
            new TableModel { TableSchema = "dbo", TableName = "Logs", TableType = "BASE TABLE" }
        };
        var columns = new List<ColumnModel>
        {
            new ColumnModel { TableSchema = "dbo", TableName = "Logs", ColumnName = "LogId", DataType = "int", OrdinalPosition = "1" }
        };
        var procedures = new List<string> { "sp_WriteLog" };

        // Act
        await writer.WriteConnectionStringAsync("Server=localhost;Database=LogsDB;");
        await writer.WriteTablesAsync(tables);
        await writer.WriteColumnsAsync(columns);
        await writer.WriteStoredProceduresAsync(procedures);
        await writer.SaveAsync();

        // Assert
        Assert.True(File.Exists(_tempFilePath));
        string json = File.ReadAllText(_tempFilePath);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(json, options);

        Assert.NotNull(schema);
        Assert.Equal("Server=localhost;Database=LogsDB;", schema.ConnectionString);
        Assert.Single(schema.Tables);
        Assert.Equal("Logs", schema.Tables[0].TableName);
        Assert.Single(schema.Tables[0].Columns);
        Assert.Equal("LogId", schema.Tables[0].Columns[0].ColumnName);
        Assert.Single(schema.StoredProcedures);
        Assert.Equal("sp_WriteLog", schema.StoredProcedures[0]);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}
