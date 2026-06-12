using System.Text.Json;
using PRS.Commands;
using PRS.Database;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class DumpDatabaseSchemaCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _tempSchemasDir;

    public DumpDatabaseSchemaCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        
        // Set APPDATA to test temp path FIRST
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
        
        // Now create necessary directories
        var prsDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        _tempSchemasDir = Path.Combine(prsDir, "schemas");
        Directory.CreateDirectory(_tempSchemasDir);
        
        // Create a default connection string file for tests
        var connStringPath = Path.Combine(prsDir, "prs.txt");
        File.WriteAllText(connStringPath, "Server=localhost;Database=TestDB;Integrated Security=true;");
    }

    [Fact]
    public async Task RunAsync_CreatesJsonSchemaFile()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(File.Exists(schemaFilePath), "Schema file should be created");
        Assert.True(_display.ContainsInfo("Dump database schema has been done"));
    }

    [Fact]
    public async Task RunAsync_CreatesValidJsonDocument()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(content, options);

        Assert.NotNull(schema);
        Assert.Equal("Server=localhost;Database=TestDB;Integrated Security=true;", schema.ConnectionString);
        Assert.NotEmpty(schema.Tables);
        Assert.NotEmpty(schema.Tables.SelectMany(t => t.Columns));
        Assert.NotEmpty(schema.StoredProcedures);
    }

    [Fact]
    public async Task RunAsync_WritesTablesCorrectly()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(content, options);

        Assert.NotNull(schema);
        Assert.Contains(schema.Tables, t => t.TableName == "Users");
        Assert.Contains(schema.Tables, t => t.TableName == "Orders");
        Assert.Contains(schema.Tables, t => t.TableName == "Products");
    }

    [Fact]
    public async Task RunAsync_WritesColumnsUnderTables()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(content, options);

        Assert.NotNull(schema);
        var allColumns = schema.Tables.SelectMany(t => t.Columns.Select(c => new { t.TableName, c.ColumnName }));
        Assert.Contains(allColumns, c => c.TableName == "Users" && c.ColumnName == "UserId");
        Assert.Contains(allColumns, c => c.TableName == "Users" && c.ColumnName == "UserName");
    }

    [Fact]
    public async Task RunAsync_WritesForeignKeyInformation()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(content, options);

        Assert.NotNull(schema);
        var ordersTable = schema.Tables.FirstOrDefault(t => t.TableName == "Orders");
        Assert.NotNull(ordersTable);
        var fkColumn = ordersTable.Columns.FirstOrDefault(c => c.ColumnName == "UserId");
        Assert.NotNull(fkColumn);
        Assert.Equal("Users", fkColumn.ReferencedTableName);
        Assert.Equal("UserId", fkColumn.ReferencedColumnName);
    }

    [Fact]
    public async Task RunAsync_WritesStoredProcedures()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(content, options);

        Assert.NotNull(schema);
        Assert.Contains("sp_GetUsers", schema.StoredProcedures);
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var args = new[] { "dds" }; // missing schema name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_OverwritesExistingFile()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.json");
        var args = new[] { "dds", schemaName };

        // Create existing file
        await File.WriteAllTextAsync(schemaFilePath, "{\"connectionString\": \"old\"}");

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var schema = JsonSerializer.Deserialize<JsonSchemaModel>(content, options);

        Assert.NotNull(schema);
        Assert.Equal("Server=localhost;Database=TestDB;Integrated Security=true;", schema.ConnectionString);
    }

    [Fact]
    public async Task RunAsync_SetsActiveSchemaPointer()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var expectedPointerValue = $"{schemaName}.schema.json";
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var activePointerPath = Path.Combine(TestFileHelper.GetTempPath(), ".prs", "active.txt");
        Assert.True(File.Exists(activePointerPath), "Active pointer file should be created");

        var activeName = (await File.ReadAllTextAsync(activePointerPath)).Trim();
        Assert.Equal(expectedPointerValue, activeName);
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}
