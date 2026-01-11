using PRS.Commands;
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
    public async Task RunAsync_CreatesMarkdownSchemaFile()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(File.Exists(schemaFilePath), "Schema file should be created");
        Assert.True(_display.ContainsInfo("Dump database schema has been done"));
    }

    [Fact]
    public async Task RunAsync_CreatesValidMarkdownDocument()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.Contains("# Database Schema", content);
        Assert.Contains("## Connection String", content);
        Assert.Contains("## Tables", content);
        Assert.Contains("## Stored Procedures", content);
    }

    [Fact]
    public async Task RunAsync_WritesTablesCorrectly()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.Contains("### Users", content);
        Assert.Contains("### Orders", content);
        Assert.Contains("### Products", content);
    }

    [Fact]
    public async Task RunAsync_WritesColumnsUnderTables()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.Contains("### Users", content);
        Assert.Contains("UserId", content);
        Assert.Contains("UserName", content);
    }

    [Fact]
    public async Task RunAsync_WritesForeignKeyInformation()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.Contains("### Orders", content);
        Assert.Contains("FK_Orders_Users", content);
        Assert.Contains("→ Users.UserId", content);
    }

    [Fact]
    public async Task RunAsync_WritesStoredProcedures()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.Contains("## Stored Procedures", content);
        Assert.Contains("- sp_GetUsers", content);
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
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.md");
        var args = new[] { "dds", schemaName };

        // Create existing file
        await File.WriteAllTextAsync(schemaFilePath, "# Old Data");

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.DoesNotContain("# Old Data", content);
        Assert.Contains("# Database Schema", content);
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

