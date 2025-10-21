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
    public async Task RunAsync_ShowsSchemaWithoutExtension()
    {
        // Arrange
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/mydatabase.schema.md");

        var command = new ListSchemasCommand(_display);
        var args = new[] { "ls" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("mydatabase"));
        Assert.False(_display.AllMessages.Any(m => m.Contains(".schema.md")));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

