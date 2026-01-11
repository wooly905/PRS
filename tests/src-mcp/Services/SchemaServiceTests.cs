using PRS.FileHandle;
using PRS.McpServer.Services;
using Xunit;

namespace PRS.McpServer.Tests.Services;

public class SchemaServiceTests
{
    [Fact]
    public async Task ListSchemasAsync_WhenNoSchemasExist_ReturnsEmptyList()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var service = new SchemaService(fileProvider, "non-existent.schema.md");

        // Act
        var result = await service.ListSchemasAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Schemas);
    }

    [Fact]
    public async Task FindTablesAsync_WhenSchemaNotExists_ReturnsEmptyList()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var service = new SchemaService(fileProvider, "non-existent.schema.md");

        // Act
        var result = await service.FindTablesAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindColumnsAsync_WhenSchemaNotExists_ReturnsEmptyList()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var service = new SchemaService(fileProvider, "non-existent.schema.md");

        // Act
        var result = await service.FindColumnsAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindStoredProceduresAsync_WhenSchemaNotExists_ReturnsEmptyList()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var service = new SchemaService(fileProvider, "non-existent.schema.md");

        // Act
        var result = await service.FindStoredProceduresAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTableDetailsAsync_WhenSchemaNotExists_ReturnsEmptyList()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var service = new SchemaService(fileProvider, "non-existent.schema.md");

        // Act
        var result = await service.GetTableDetailsAsync("TestTable");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SwitchSchemaAsync_WhenSchemaNotExists_ReturnsFailure()
    {
        // Arrange
        IFileProvider fileProvider = new FileProvider();
        var service = new SchemaService(fileProvider, "non-existent.schema.md");

        // Act
        var result = await service.SwitchSchemaAsync("NonExistentSchema");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}

