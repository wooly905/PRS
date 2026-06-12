using PRS.Commands;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class UseSchemaCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly string _tempSchemasDir;

    public UseSchemaCommandTests()
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
    public async Task RunAsync_WithValidSchemaName_SwitchesActiveSchema()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb.schema.md");

        var command = new UseSchemaCommand(_display);
        var args = new[] { "use", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Active schema switched to: testdb"));
    }

    [Fact]
    public async Task RunAsync_WithBaseNameOnly_SwitchesActiveSchema()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/prod.schema.md");

        var command = new UseSchemaCommand(_display);
        var args = new[] { "use", "prod" }; // Use simple name without extension

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Active schema switched to: prod"));
        Assert.False(_display.ContainsError("Schema not found"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentSchema_ShowsError()
    {
        // Arrange
        var command = new UseSchemaCommand(_display);
        var args = new[] { "use", "nonexistent" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Schema not found"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new UseSchemaCommand(_display);
        var args = new[] { "use" }; // missing schema name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new UseSchemaCommand(_display);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithJsonSchemaFile_SwitchesActiveSchema()
    {
        // Arrange: create a .json schema file
        var jsonPath = Path.Combine(_tempSchemasDir, "testdb.schema.json");
        File.WriteAllText(jsonPath, "{\"connectionString\":\"\",\"tables\":[],\"storedProcedures\":[]}");

        var command = new UseSchemaCommand(_display);
        var args = new[] { "use", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert: should find the .json file
        Assert.True(_display.ContainsAnyMessage("Active schema switched to: testdb"));
        Assert.False(_display.ContainsError("Schema not found"));
    }

    [Fact]
    public async Task RunAsync_BothJsonAndMdExist_PrefersJson()
    {
        // Arrange: create both .json and .md
        File.WriteAllText(
            Path.Combine(_tempSchemasDir, "testdb.schema.json"),
            "{\"connectionString\":\"\",\"tables\":[],\"storedProcedures\":[]}");
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb.schema.md");

        var command = new UseSchemaCommand(_display);
        var args = new[] { "use", "testdb" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Active schema switched to: testdb"));

        // Verify pointer points to .json
        var activePointerPath = Path.Combine(TestFileHelper.GetTempPath(), ".prs", "active.txt");
        var activeName = File.ReadAllText(activePointerPath).Trim();
        Assert.EndsWith(".schema.json", activeName);
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

