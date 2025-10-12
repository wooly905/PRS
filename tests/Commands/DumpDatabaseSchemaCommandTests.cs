using PRS.Commands;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using System.Xml.Linq;
using Xunit;

namespace PRS.Tests.Commands;

public class DumpDatabaseSchemaCommandTests : IDisposable
{
    private readonly TestDisplay _display;
    private readonly IFileProvider _fileProvider;
    private readonly string _tempSchemasDir;

    public DumpDatabaseSchemaCommandTests()
    {
        _display = new TestDisplay();
        _fileProvider = new FileProvider();
        
        // Set APPDATA to test temp path FIRST
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
        
        // Now create necessary directories
        var prsDir = Path.Combine(TestFileHelper.GetTempPath(), ".prs");
        _tempSchemasDir = Path.Combine(prsDir, "schemas");
        Directory.CreateDirectory(_tempSchemasDir);
        
        // Create a default connection string file for tests
        var connStringPath = Path.Combine(prsDir, "prs.txt");
        File.WriteAllText(connStringPath, "Server=localhost;Database=TestDB;Integrated Security=true;");
    }

    [Fact]
    public async Task RunAsync_CreatesXmlSchemaFile()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(File.Exists(schemaFilePath), "Schema file should be created");
        Assert.True(_display.ContainsInfo("Dump database schema has been done"));
    }

    [Fact]
    public async Task RunAsync_CreatesValidXmlDocument()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var xdoc = XDocument.Load(schemaFilePath);
        Assert.NotNull(xdoc.Root);
        Assert.Equal("Databases", xdoc.Root.Name.LocalName);

        var database = xdoc.Root.Element("Database");
        Assert.NotNull(database);
        Assert.NotNull(database.Element("ConnectionString"));
        Assert.NotNull(database.Element("Tables"));
        Assert.NotNull(database.Element("StoredProcedures"));
    }

    [Fact]
    public async Task RunAsync_WritesTablesCorrectly()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var xdoc = XDocument.Load(schemaFilePath);
        var tables = xdoc.Descendants("Table").ToList();
        Assert.Equal(3, tables.Count);
        Assert.Contains(tables, t => t.Element("Name")?.Value == "Users");
        Assert.Contains(tables, t => t.Element("Name")?.Value == "Orders");
        Assert.Contains(tables, t => t.Element("Name")?.Value == "Products");
    }

    [Fact]
    public async Task RunAsync_WritesColumnsUnderTables()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var xdoc = XDocument.Load(schemaFilePath);
        var usersTable = xdoc.Descendants("Table")
            .FirstOrDefault(t => t.Element("Name")?.Value == "Users");
        Assert.NotNull(usersTable);

        var columns = usersTable.Element("Columns")?.Elements("Column").ToList();
        Assert.NotNull(columns);
        Assert.NotEmpty(columns);
        Assert.Contains(columns, c => c.Element("Name")?.Value == "UserId");
        Assert.Contains(columns, c => c.Element("Name")?.Value == "UserName");
    }

    [Fact]
    public async Task RunAsync_WritesForeignKeyInformation()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var xdoc = XDocument.Load(schemaFilePath);
        var ordersTable = xdoc.Descendants("Table")
            .FirstOrDefault(t => t.Element("Name")?.Value == "Orders");
        var userIdColumn = ordersTable?.Element("Columns")?.Elements("Column")
            .FirstOrDefault(c => c.Element("Name")?.Value == "UserId");

        Assert.NotNull(userIdColumn);
        var fk = userIdColumn.Element("ForeignKey");
        Assert.NotNull(fk);
        Assert.Equal("FK_Orders_Users", fk.Element("Name")?.Value);
        Assert.Equal("Users", fk.Element("ReferencedTableName")?.Value);
    }

    [Fact]
    public async Task RunAsync_WritesStoredProcedures()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Act
        await command.RunAsync(args);

        // Assert
        var xdoc = XDocument.Load(schemaFilePath);
        var procedures = xdoc.Descendants("Procedure").ToList();
        Assert.Equal(3, procedures.Count);
        Assert.Contains(procedures, p => p.Element("Name")?.Value == "sp_GetUsers");
    }

    [Fact]
    public async Task RunAsync_WithMissingArguments_ShowsError()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var args = new[] { "dds" }; // missing schema name

        // Act
        await command.RunAsync(args);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);

        // Act
        await command.RunAsync(null);

        // Assert
        Assert.True(_display.ContainsError("Argument mismatch"));
    }

    [Fact]
    public async Task RunAsync_OverwritesExistingFile()
    {
        // Arrange
        var mockDb = MockDatabase.CreateTestData();
        var command = new DumpDatabaseSchemaCommand(_display, mockDb, _fileProvider);
        var schemaName = "testdb";
        var schemaFilePath = Path.Combine(_tempSchemasDir, $"{schemaName}.schema.xml");
        var args = new[] { "dds", schemaName };

        // Create existing file
        await File.WriteAllTextAsync(schemaFilePath, "<OldData></OldData>");

        // Act
        await command.RunAsync(args);

        // Assert
        var content = await File.ReadAllTextAsync(schemaFilePath);
        Assert.DoesNotContain("OldData", content);
        Assert.Contains("Databases", content);
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

