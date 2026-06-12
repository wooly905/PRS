using PRS.Commands;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class ListSchemasCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly string _tempSchemasDir;

    public ListSchemasCommandTests()
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
    public async Task RunAsync_WithNoSchemas_ShowsNothingFound()
    {
        // Arrange
        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsInfo("No schemas found"));
    }

    [Fact]
    public async Task RunAsync_WithMultipleSchemas_ListsAllSchemas()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb1.schema.md");
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb2.schema.md");
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/prod.schema.md");

        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Saved schemas:"));
        Assert.True(_display.ContainsAnyMessage("testdb1"));
        Assert.True(_display.ContainsAnyMessage("testdb2"));
        Assert.True(_display.ContainsAnyMessage("prod"));
    }

    [Fact]
    public async Task RunAsync_MarksActiveSchema()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb1.schema.md");
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/testdb2.schema.md");

        // Set active schema
        var activePointerDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        Directory.CreateDirectory(activePointerDir);
        var activePointerPath = Path.Combine(activePointerDir, "active.txt");
        await File.WriteAllTextAsync(activePointerPath, "testdb1.schema.md");

        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("testdb1 (active)"));
        Assert.True(_display.AllMessages.Any(m => m.Contains("testdb2") && !m.Contains("(active)")));
    }

    [Fact]
    public async Task RunAsync_WithJsonSchemas_ListsWithoutExtension()
    {
        // Arrange: create .json schema files
        File.WriteAllText(Path.Combine(_tempSchemasDir, "prod.schema.json"), "{}");
        File.WriteAllText(Path.Combine(_tempSchemasDir, "staging.schema.json"), "{}");

        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Saved schemas:"));
        Assert.True(_display.ContainsAnyMessage("prod"));
        Assert.True(_display.ContainsAnyMessage("staging"));
        // Should NOT show the full extension
        Assert.False(_display.AllMessages.Any(m => m.Contains(".schema.json")));
    }

    [Fact]
    public async Task RunAsync_WithMixedJsonAndMd_ListsBoth()
    {
        // Arrange: mix of .json and .md
        File.WriteAllText(Path.Combine(_tempSchemasDir, "db1.schema.json"), "{}");
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/db2.schema.md");

        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("db1"));
        Assert.True(_display.ContainsAnyMessage("db2"));
    }

    [Fact]
    public async Task RunAsync_JsonSchemaActive_ShowsActiveMarker()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempSchemasDir, "prod.schema.json"), "{}");
        File.WriteAllText(Path.Combine(_tempSchemasDir, "staging.schema.json"), "{}");

        // Set active schema
        var activePointerDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        Directory.CreateDirectory(activePointerDir);
        await File.WriteAllTextAsync(
            Path.Combine(activePointerDir, "active.txt"), "prod.schema.json");

        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("prod (active)"));
        Assert.True(_display.AllMessages.Any(m => m.Contains("staging") && !m.Contains("(active)")));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

