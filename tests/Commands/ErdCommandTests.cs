using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class ErdCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _testSchemaPath;

    public ErdCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        _testSchemaPath = TestFileHelper.CreateTestSchemaFile("test.schema.xml");
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new ErdCommand(_display, _fileProvider);
        var args = new[] { "erd" }; // missing table name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new ErdCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentSchemaFile_ShowsError()
    {
        // Arrange
        var command = new ErdCommand(_display, _fileProvider);
        var args = new[] { "erd", "Users" };

        // Set environment to point to non-existent path
        var tempDir = Path.Combine(TestFileHelper.GetTempPath(), "nonexistent");
        Environment.SetEnvironmentVariable("APPDATA", tempDir);

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Schema doesn't exist") || _display.ContainsError("not found"));
    }

    // Note: We cannot easily test the Terminal.Gui UI parts in unit tests
    // The UI would require a terminal/console environment and user interaction
    // Therefore, we focus on testing the error cases and validation logic

    [Fact]
    public void Constructor_WithValidParameters_DoesNotThrow()
    {
        // Arrange & Act
        var command = new ErdCommand(_display, _fileProvider);

        // Assert
        Assert.NotNull(command);
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

