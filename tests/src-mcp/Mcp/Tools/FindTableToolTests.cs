using PRS.FileHandle;
using PRS.McpServer.Mcp.Tools;
using PRS.McpServer.Services;
using Xunit;

namespace PRS.McpServer.Tests.Mcp.Tools;

public class FindTableToolTests
{
    [Fact]
    public void GetToolDefinition_ReturnsCorrectDefinition()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var schemaService = new SchemaService(fileProvider);
        var tool = new FindTableTool(schemaService);

        // Act
        var definition = tool.GetToolDefinition();

        // Assert
        Assert.NotNull(definition);
        Assert.Equal("find_table", tool.Name);
        Assert.Contains("Search for tables", tool.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WhenKeywordMissing_ThrowsArgumentException()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var schemaService = new SchemaService(fileProvider);
        var tool = new FindTableTool(schemaService);
        var arguments = new Dictionary<string, object?>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tool.ExecuteAsync(arguments));
    }

    [Fact]
    public async Task ExecuteAsync_WhenSchemaNotExists_ReturnsEmptyList()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var schemaService = new SchemaService(fileProvider);
        var tool = new FindTableTool(schemaService);
        var arguments = new Dictionary<string, object?>
        {
            { "keyword", "test" }
        };

        // Act
        var result = await tool.ExecuteAsync(arguments);

        // Assert
        Assert.NotNull(result);
    }
}

