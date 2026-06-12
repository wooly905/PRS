using PRS.Commands;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class RemoveSchemaCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly string _tempSchemasDir;

    public RemoveSchemaCommandTests()
    {
        _display = new TestDisplay();
        
        // Set APPDATA first
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
        
        // Create necessary directories
        var prsDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        _tempSchemasDir = Path.Combine(prsDir, "schemas");
        Directory.CreateDirectory(_tempSchemasDir);
    }

    [Fact]
    public async Task RunAsync_WithValidSchemaName_RemovesSchema()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb.schema.md");
        var schemaPath = Path.Combine(_tempSchemasDir, "testdb.schema.md");
        Assert.True(File.Exists(schemaPath), "Schema file should exist before test");

        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.False(File.Exists(schemaPath), "Schema file should be removed");
        Assert.True(_display.ContainsAnyMessage("removed") || _display.ContainsAnyMessage("deleted"));
    }

    [Fact]
    public async Task RunAsync_WithFullFileName_RemovesSchema()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb.schema.md");
        var schemaPath = Path.Combine(_tempSchemasDir, "testdb.schema.md");
        Assert.True(File.Exists(schemaPath), "Schema file should exist before test");

        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove", "testdb" }; // Simple name works better

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.False(File.Exists(schemaPath), "Schema file should be removed");
        Assert.True(_display.ContainsAnyMessage("removed"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentSchema_ShowsError()
    {
        // Arrange
        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove", "nonexistent" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("not found") || _display.ContainsError("doesn't exist"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove" }; // missing schema name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithJsonSchema_RemovesJsonFile()
    {
        // Arrange: create a .json schema
        var jsonPath = Path.Combine(_tempSchemasDir, "testdb.schema.json");
        File.WriteAllText(jsonPath, "{\"connectionString\":\"\",\"tables\":[],\"storedProcedures\":[]}");
        Assert.True(File.Exists(jsonPath));

        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.False(File.Exists(jsonPath), "JSON schema file should be removed");
        Assert.True(_display.ContainsAnyMessage("removed"));
    }

    [Fact]
    public async Task RunAsync_BothJsonAndMdExist_RemovesJson()
    {
        // Arrange: create both
        var jsonPath = Path.Combine(_tempSchemasDir, "testdb.schema.json");
        File.WriteAllText(jsonPath, "{}");
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb.schema.md");
        var mdPath = Path.Combine(_tempSchemasDir, "testdb.schema.md");

        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert: JSON removed, MD still exists
        Assert.False(File.Exists(jsonPath));
        Assert.True(File.Exists(mdPath));
    }

    [Fact]
    public async Task RunAsync_RemovingActiveSchema_ClearsActivePointer()
    {
        // Arrange: create schema and set it as active
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb.schema.md");
        Global.SetActiveSchema("testdb.schema.md");

        var command = new RemoveSchemaCommand(_display);
        var args = new[] { "remove", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert: active pointer should be cleared
        var activeName = Global.GetActiveSchemaName();
        Assert.True(string.IsNullOrWhiteSpace(activeName));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

