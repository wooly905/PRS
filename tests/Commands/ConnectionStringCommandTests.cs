using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class ConnectionStringCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _tempDataDir;

    public ConnectionStringCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        _tempDataDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        Directory.CreateDirectory(_tempDataDir);
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
    }

    [Fact]
    public async Task WriteConnectionStringCommand_WithValidConnectionString_WritesFile()
    {
        // Arrange
        var command = new WriteConnectionStringCommand(_display, _fileProvider);
        var connectionString = "Server=localhost;Database=TestDB;Integrated Security=true;";
        var args = new[] { "wcs", connectionString };

        // Act
        await command.RunAsync(args);

        // Assert
        var connStringPath = Path.Combine(_tempDataDir, "prs.txt");
        Assert.True(File.Exists(connStringPath), "Connection string file should be created");

        var savedConnString = (await File.ReadAllTextAsync(connStringPath)).Trim();
        Assert.Equal(connectionString, savedConnString);
        Assert.True(_display.ContainsAnyMessage("Connection string has been set"));
    }

    [Fact]
    public async Task WriteConnectionStringCommand_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new WriteConnectionStringCommand(_display, _fileProvider);
        var args = new[] { "wcs" }; // missing connection string

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Connection string argument is missing") || 
                    _display.ContainsAnyMessage("Argument mismatch"));
    }

    [Fact]
    public async Task WriteConnectionStringCommand_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new WriteConnectionStringCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Connection string argument is missing") || 
                    _display.ContainsAnyMessage("Argument mismatch"));
    }

    [Fact]
    public async Task WriteConnectionStringCommand_OverwritesExistingFile()
    {
        // Arrange
        var connStringPath = Path.Combine(_tempDataDir, "prs.txt");
        await File.WriteAllTextAsync(connStringPath, "OldConnectionString");

        var command = new WriteConnectionStringCommand(_display, _fileProvider);
        var newConnectionString = "Server=newserver;Database=NewDB;";
        var args = new[] { "wcs", newConnectionString };

        // Act
        await command.RunAsync(args);

        // Assert
        var savedConnString = (await File.ReadAllTextAsync(connStringPath)).Trim();
        Assert.Equal(newConnectionString, savedConnString);
        Assert.DoesNotContain("OldConnectionString", savedConnString);
    }

    [Fact]
    public async Task ShowConnectionStringCommand_WithExistingFile_ShowsConnectionString()
    {
        // Arrange
        var connStringPath = Path.Combine(_tempDataDir, "prs.txt");
        var connectionString = "Server=localhost;Database=TestDB;";
        await File.WriteAllTextAsync(connStringPath, connectionString);

        var command = new ShowConnectionStringCommand(_display, _fileProvider);
        var args = new[] { "showcs" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage(connectionString));
    }

    [Fact]
    public async Task ShowConnectionStringCommand_WithNonExistentFile_ShowsError()
    {
        // Arrange
        var command = new ShowConnectionStringCommand(_display, _fileProvider);
        var args = new[] { "showcs" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("Connection string doesn't exist") || 
                    _display.ContainsAnyMessage("not found"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

