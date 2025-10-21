using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class FindStoredProcedureCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _testSchemaPath;

    public FindStoredProcedureCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        
        // Set APPDATA to test temp path
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
        
        // Create test schema file in the location Global expects
        TestFileHelper.CopyTestFile("test.schema.md", ".prs/schemas/schema.md");
        _testSchemaPath = Path.Combine(TestFileHelper.GetTempPath(), ".prs", "schemas", "schema.md");
    }

    [Fact]
    public async Task RunAsync_WithValidProcedureName_FindsMatches()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp", "GetUser" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("sp_GetUserOrders") || _display.ContainsAnyMessage("sp_GetUserById"));
        Assert.False(_display.ContainsAnyMessage("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithPartialName_FindsAllMatches()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp", "sp_Get" }; // More specific prefix

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("sp_GetUserOrders"));
        Assert.True(_display.ContainsAnyMessage("sp_GetUserById"));
        Assert.False(_display.ContainsAnyMessage("sp_CreateOrder")); // Different verb
        Assert.False(_display.ContainsAnyMessage("usp_")); // Different prefix
    }

    [Fact]
    public async Task RunAsync_WithCaseInsensitiveSearch_FindsProcedures()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp", "getuser" }; // lowercase

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("GetUser"));
    }

    [Fact]
    public async Task RunAsync_WithNonExistentProcedure_ShowsNothingFound()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp", "NonExistentProcedure" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsInfo("Nothing found"));
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp" }; // missing procedure name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_FindsProceduresWithDifferentPrefixes()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp", "usp" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("usp_CalculateTotal"));
    }

    [Fact]
    public async Task RunAsync_WithCommonVerb_FindsMultipleProcedures()
    {
        // Arrange
        var command = new FindStoredProcedureCommand(_display, _fileProvider);
        var args = new[] { "fsp", "Get" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("sp_GetUserOrders"));
        Assert.True(_display.ContainsAnyMessage("sp_GetUserById"));
        Assert.False(_display.ContainsAnyMessage("sp_CreateOrder"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

