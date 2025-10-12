using PRS.FileHandle;
using PRS.Services;
using PRS.Tests.TestHelpers;
using Xunit;

namespace PRS.Tests.Services;

public class SchemaSearchServiceTests : IDisposable
{
    private readonly IFileProvider _fileProvider;
    private readonly SchemaSearchService _service;
    private readonly string _testSchemaPath;

    public SchemaSearchServiceTests()
    {
        _fileProvider = new FileProvider();
        _service = new SchemaSearchService(_fileProvider);
        _testSchemaPath = TestFileHelper.CreateTestSchemaFile("test.schema.xml");
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithEmptyJson_ReturnsAllTablesAndColumns()
    {
        // Arrange
        var extractionJson = "{}";

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        Assert.NotNull(context);
        Assert.Contains("TABLE|", context);
        Assert.Contains("COLUMN|", context);
        Assert.Contains("Users", context);
        Assert.Contains("Orders", context);
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithTableHints_FiltersTablesByHint()
    {
        // Arrange
        var extractionJson = """
        {
            "candidateTables": ["Users", "Orders"]
        }
        """;

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        Assert.Contains("TABLE|dbo|Users", context);
        Assert.Contains("TABLE|dbo|Orders", context);
        // Should still include columns from all tables that match
        Assert.Contains("COLUMN|", context);
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithColumnHints_FiltersColumnsByHint()
    {
        // Arrange
        var extractionJson = """
        {
            "candidateColumns": ["UserId", "OrderId"]
        }
        """;

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        Assert.Contains("COLUMN|dbo|Users|UserId", context);
        Assert.Contains("COLUMN|dbo|Orders|OrderId", context);
        // Should not include unrelated columns like ProductName
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithBothHints_FiltersAccordingly()
    {
        // Arrange
        var extractionJson = """
        {
            "candidateTables": ["Users"],
            "candidateColumns": ["Email"]
        }
        """;

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        Assert.Contains("TABLE|dbo|Users", context);
        Assert.Contains("COLUMN|dbo|Users|Email", context);
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithInvalidJson_DoesNotThrow()
    {
        // Arrange
        var extractionJson = "invalid json";

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        Assert.NotNull(context);
        // Should return all tables and columns as fallback
        Assert.Contains("TABLE|", context);
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithNullJson_ReturnsAllData()
    {
        // Arrange
        string extractionJson = null;

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        Assert.NotNull(context);
        Assert.Contains("TABLE|", context);
        Assert.Contains("COLUMN|", context);
    }

    [Fact]
    public async Task BuildSchemaContextAsync_IncludesDataTypeInColumnInfo()
    {
        // Arrange
        var extractionJson = "{}";

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        // Format: COLUMN|schema|table|column|datatype
        var lines = context.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var columnLines = lines.Where(l => l.StartsWith("COLUMN|")).ToList();
        Assert.NotEmpty(columnLines);

        // Check format
        var sampleColumnLine = columnLines.FirstOrDefault(l => l.Contains("UserId"));
        Assert.NotNull(sampleColumnLine);
        var parts = sampleColumnLine.Split('|');
        Assert.Equal(5, parts.Length); // COLUMN | schema | table | column | datatype
    }

    [Fact]
    public void ValidateSql_WithValidSqlAndContext_ReturnsTrue()
    {
        // Arrange
        var sql = "SELECT * FROM dbo.Users";
        var context = """
        TABLE|dbo|Users|BASE TABLE
        COLUMN|dbo|Users|UserId|int
        """;

        // Act
        var isValid = SchemaSearchService.ValidateSql(sql, context);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateSql_WithInvalidTable_ReturnsFalse()
    {
        // Arrange
        var sql = "SELECT * FROM dbo.InvalidTable";
        var context = """
        TABLE|dbo|Users|BASE TABLE
        COLUMN|dbo|Users|UserId|int
        """;

        // Act
        var isValid = SchemaSearchService.ValidateSql(sql, context);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSql_WithJoinToValidTables_ReturnsTrue()
    {
        // Arrange
        var sql = "SELECT * FROM dbo.Users u JOIN dbo.Orders o ON u.UserId = o.UserId";
        var context = """
        TABLE|dbo|Users|BASE TABLE
        TABLE|dbo|Orders|BASE TABLE
        COLUMN|dbo|Users|UserId|int
        COLUMN|dbo|Orders|UserId|int
        """;

        // Act
        var isValid = SchemaSearchService.ValidateSql(sql, context);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateSql_WithEmptySql_ReturnsFalse()
    {
        // Arrange
        var sql = "";
        var context = "TABLE|dbo|Users|BASE TABLE";

        // Act
        var isValid = SchemaSearchService.ValidateSql(sql, context);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSql_WithEmptyContext_ReturnsFalse()
    {
        // Arrange
        var sql = "SELECT * FROM dbo.Users";
        var context = "";

        // Act
        var isValid = SchemaSearchService.ValidateSql(sql, context);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task BuildSchemaContextAsync_WithPartialTableNameHint_MatchesPartial()
    {
        // Arrange
        var extractionJson = """
        {
            "candidateTables": ["User"]
        }
        """;

        // Act
        var context = await _service.BuildSchemaContextAsync(_testSchemaPath, extractionJson);

        // Assert
        // Should match Users and UserRoles (contains "User")
        Assert.Contains("Users", context);
        Assert.Contains("UserRoles", context);
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

