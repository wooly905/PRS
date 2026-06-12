using Moq;
using PRS.Commands;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.Commands;

public class CommandProviderTests
{
    private readonly Mock<IDisplay> _mockDisplay = new();
    private readonly Mock<IDatabase> _mockDatabase = new();
    private readonly Mock<IFileProvider> _mockFileProvider = new();

    [Theory]
    [InlineData("lt")]
    [InlineData("LT")]
    [InlineData("Lt")]
    public void TryGetProvider_Lt_ReturnsListTablesCommand(string input)
    {
        bool result = CommandProvider.TryGetProvider(
            input, _mockDisplay.Object, _mockDatabase.Object, _mockFileProvider.Object, out var command);

        Assert.True(result);
        Assert.IsType<ListTablesCommand>(command);
    }

    [Theory]
    [InlineData("ft", typeof(FindTableCommand))]
    [InlineData("fc", typeof(FindColumnCommand))]
    [InlineData("ftc", typeof(FindTableColumnCommand))]
    [InlineData("fsp", typeof(FindStoredProcedureCommand))]
    [InlineData("sc", typeof(ShowAllColumnsCommand))]
    [InlineData("dds", typeof(DumpDatabaseSchemaCommand))]
    [InlineData("ls", typeof(ListSchemasCommand))]
    [InlineData("use", typeof(UseSchemaCommand))]
    [InlineData("rm", typeof(RemoveSchemaCommand))]
    [InlineData("scs", typeof(ShowConnectionStringCommand))]
    [InlineData("wcs", typeof(WriteConnectionStringCommand))]
    public void TryGetProvider_ValidInput_ReturnsExpectedType(string input, Type expectedType)
    {
        bool result = CommandProvider.TryGetProvider(
            input, _mockDisplay.Object, _mockDatabase.Object, _mockFileProvider.Object, out var command);

        Assert.True(result);
        Assert.IsType(expectedType, command);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData("xyz")]
    public void TryGetProvider_InvalidInput_ReturnsFalse(string input)
    {
        bool result = CommandProvider.TryGetProvider(
            input, _mockDisplay.Object, _mockDatabase.Object, _mockFileProvider.Object, out var command);

        Assert.False(result);
        Assert.Null(command);
    }
}
