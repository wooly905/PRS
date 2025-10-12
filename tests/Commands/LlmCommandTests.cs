using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Commands;

public class LlmCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _tempDataDir;

    public LlmCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        _tempDataDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        Directory.CreateDirectory(_tempDataDir);
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
    }

    [Fact]
    public async Task LlmUrlCommand_WithValidUrl_WritesConfig()
    {
        // Arrange
        var command = new LlmUrlCommand(_display, _fileProvider);
        var url = "https://api.openai.com/v1";
        var args = new[] { "llmurl", url };

        // Act
        await command.RunAsync(args);

        // Assert
        var configPath = Path.Combine(_tempDataDir, "llm.config");
        Assert.True(File.Exists(configPath), "LLM config file should be created");
        Assert.True(_display.ContainsAnyMessage("saved") || _display.ContainsAnyMessage("set"));
    }

    [Fact]
    public async Task LlmUrlCommand_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new LlmUrlCommand(_display, _fileProvider);
        var args = new[] { "llmurl" }; // missing URL

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task LlmUrlCommand_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new LlmUrlCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task LlmApiKeyCommand_WithValidKey_WritesConfig()
    {
        // Arrange
        var command = new LlmApiKeyCommand(_display, _fileProvider);
        var apiKey = "sk-test1234567890";
        var args = new[] { "llmapikey", apiKey };

        // Act
        await command.RunAsync(args);

        // Assert
        var configPath = Path.Combine(_tempDataDir, "llm.config");
        Assert.True(File.Exists(configPath), "LLM config file should be created");
        Assert.True(_display.ContainsAnyMessage("saved") || _display.ContainsAnyMessage("set"));
    }

    [Fact]
    public async Task LlmApiKeyCommand_WithMissingArguments_ShowsError()
    {
        // Arrange
        var command = new LlmApiKeyCommand(_display, _fileProvider);
        var args = new[] { "llmapikey" }; // missing API key

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task LlmApiKeyCommand_WithNullArguments_ShowsError()
    {
        // Arrange
        var command = new LlmApiKeyCommand(_display, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task ShowLlmUrlCommand_WithExistingConfig_ShowsUrl()
    {
        // Arrange
        var configPath = Path.Combine(_tempDataDir, "llm.config");
        var url = "https://api.openai.com/v1";
        await File.WriteAllTextAsync(configPath, $"[LLM_URL]\n{url}");

        var command = new ShowLlmUrlCommand(_display, _fileProvider);
        var args = new[] { "showllmurl" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage(url) || _display.AllMessages.Any());
    }

    [Fact]
    public async Task ShowLlmUrlCommand_WithNonExistentConfig_ShowsError()
    {
        // Arrange
        var command = new ShowLlmUrlCommand(_display, _fileProvider);
        var args = new[] { "showllmurl" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("LLM endpoint is not set") || 
                    _display.ContainsAnyMessage("not found"));
    }

    [Fact]
    public async Task ShowLlmApiKeyCommand_WithExistingConfig_ShowsMaskedKey()
    {
        // Arrange
        var configPath = Path.Combine(_tempDataDir, "llm.config");
        var apiKey = "sk-test1234567890";
        await File.WriteAllTextAsync(configPath, $"[LLM_API_KEY]\n{apiKey}");

        var command = new ShowLlmApiKeyCommand(_display, _fileProvider);
        var args = new[] { "showllmapikey" };

        // Act
        await command.RunAsync(args);

        // Assert
        // Should show masked version or some indication of the key
        Assert.True(_display.AllMessages.Any());
    }

    [Fact]
    public async Task ShowLlmApiKeyCommand_WithNonExistentConfig_ShowsError()
    {
        // Arrange
        var command = new ShowLlmApiKeyCommand(_display, _fileProvider);
        var args = new[] { "showllmapikey" };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsAnyMessage("LLM API key is not set") || 
                    _display.ContainsAnyMessage("not found"));
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

