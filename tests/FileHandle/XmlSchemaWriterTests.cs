using PRS.Database;
using PRS.FileHandle;
using PRS.Tests.TestHelpers;
using System.Xml.Linq;
using Xunit;

namespace PRS.Tests.FileHandle;

public class XmlSchemaWriterTests : IDisposable
{
    private readonly string _outputPath;

    public XmlSchemaWriterTests()
    {
        _outputPath = TestFileHelper.GetTempFilePath("output.schema.xml");
    }

    [Fact]
    public async Task WriteConnectionStringAsync_WritesConnectionString()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);
        var connectionString = "Server=localhost;Database=TestDB;";

        // Act
        await writer.WriteConnectionStringAsync(connectionString);
        await writer.SaveAsync();

        // Assert
        var xdoc = XDocument.Load(_outputPath);
        var connString = xdoc.Descendants("ConnectionString").FirstOrDefault();
        Assert.NotNull(connString);
        Assert.Equal(connectionString, connString.Value);
    }

    [Fact]
    public async Task WriteTablesAsync_WritesTables()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);
        var tables = new List<TableModel>
        {
            new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" },
            new() { TableSchema = "dbo", TableName = "Orders", TableType = "BASE TABLE" }
        };

        // Act
        await writer.WriteTablesAsync(tables);
        await writer.SaveAsync();

        // Assert
        var xdoc = XDocument.Load(_outputPath);
        var tableElements = xdoc.Descendants("Table").ToList();
        Assert.Equal(2, tableElements.Count);

        var usersTable = tableElements.FirstOrDefault(t => t.Element("Name")?.Value == "Users");
        Assert.NotNull(usersTable);
        Assert.Equal("dbo", usersTable.Element("Schema")?.Value);
        Assert.Equal("BASE TABLE", usersTable.Element("Type")?.Value);
    }

    [Fact]
    public async Task WriteColumnsAsync_WritesColumnsUnderTables()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);
        var tables = new List<TableModel>
        {
            new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" }
        };
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo",
                TableName = "Users",
                ColumnName = "UserId",
                OrdinalPosition = "1",
                IsNullable = "NO",
                DataType = "int",
                ColumnDefault = "",
                CharacterMaximumLength = "",
                ForeignKeyName = "",
                ReferencedTableSchema = "",
                ReferencedTableName = "",
                ReferencedColumnName = ""
            },
            new()
            {
                TableSchema = "dbo",
                TableName = "Users",
                ColumnName = "UserName",
                OrdinalPosition = "2",
                IsNullable = "NO",
                DataType = "nvarchar",
                ColumnDefault = "",
                CharacterMaximumLength = "100",
                ForeignKeyName = "",
                ReferencedTableSchema = "",
                ReferencedTableName = "",
                ReferencedColumnName = ""
            }
        };

        // Act
        await writer.WriteTablesAsync(tables);
        await writer.WriteColumnsAsync(columns);
        await writer.SaveAsync();

        // Assert
        var xdoc = XDocument.Load(_outputPath);
        var usersTable = xdoc.Descendants("Table")
            .FirstOrDefault(t => t.Element("Name")?.Value == "Users");
        Assert.NotNull(usersTable);

        var columnElements = usersTable.Element("Columns")?.Elements("Column").ToList();
        Assert.NotNull(columnElements);
        Assert.Equal(2, columnElements.Count);

        var userIdColumn = columnElements.FirstOrDefault(c => c.Element("Name")?.Value == "UserId");
        Assert.NotNull(userIdColumn);
        Assert.Equal("int", userIdColumn.Element("DataType")?.Value);
    }

    [Fact]
    public async Task WriteColumnsAsync_WritesForeignKeyInformation()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);
        var tables = new List<TableModel>
        {
            new() { TableSchema = "dbo", TableName = "Orders", TableType = "BASE TABLE" }
        };
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo",
                TableName = "Orders",
                ColumnName = "UserId",
                OrdinalPosition = "2",
                IsNullable = "NO",
                DataType = "int",
                ColumnDefault = "",
                CharacterMaximumLength = "",
                ForeignKeyName = "FK_Orders_Users",
                ReferencedTableSchema = "dbo",
                ReferencedTableName = "Users",
                ReferencedColumnName = "UserId"
            }
        };

        // Act
        await writer.WriteTablesAsync(tables);
        await writer.WriteColumnsAsync(columns);
        await writer.SaveAsync();

        // Assert
        var xdoc = XDocument.Load(_outputPath);
        var userIdColumn = xdoc.Descendants("Column")
            .FirstOrDefault(c => c.Element("Name")?.Value == "UserId");
        Assert.NotNull(userIdColumn);

        var fk = userIdColumn.Element("ForeignKey");
        Assert.NotNull(fk);
        Assert.Equal("FK_Orders_Users", fk.Element("Name")?.Value);
        Assert.Equal("Users", fk.Element("ReferencedTableName")?.Value);
    }

    [Fact]
    public async Task WriteStoredProceduresAsync_WritesStoredProcedures()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);
        var procedures = new List<string> { "sp_GetUsers", "sp_CreateOrder", "sp_DeleteUser" };

        // Act
        await writer.WriteStoredProceduresAsync(procedures);
        await writer.SaveAsync();

        // Assert
        var xdoc = XDocument.Load(_outputPath);
        var procedureElements = xdoc.Descendants("Procedure").ToList();
        Assert.Equal(3, procedureElements.Count);
        Assert.Contains(procedureElements, p => p.Element("Name")?.Value == "sp_GetUsers");
    }

    [Fact]
    public async Task WriteCompleteSchema_CreatesValidXmlDocument()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);
        var connectionString = "Server=localhost;Database=TestDB;";
        var tables = new List<TableModel>
        {
            new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" }
        };
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo",
                TableName = "Users",
                ColumnName = "UserId",
                OrdinalPosition = "1",
                IsNullable = "NO",
                DataType = "int",
                ColumnDefault = "",
                CharacterMaximumLength = "",
                ForeignKeyName = "",
                ReferencedTableSchema = "",
                ReferencedTableName = "",
                ReferencedColumnName = ""
            }
        };
        var procedures = new List<string> { "sp_GetUsers" };

        // Act
        await writer.WriteConnectionStringAsync(connectionString);
        await writer.WriteTablesAsync(tables);
        await writer.WriteColumnsAsync(columns);
        await writer.WriteStoredProceduresAsync(procedures);
        await writer.SaveAsync();

        // Assert
        var xdoc = XDocument.Load(_outputPath);
        Assert.NotNull(xdoc.Root);
        Assert.Equal("Databases", xdoc.Root.Name.LocalName);

        var database = xdoc.Root.Element("Database");
        Assert.NotNull(database);
        Assert.NotNull(database.Element("ConnectionString"));
        Assert.NotNull(database.Element("Tables"));
        Assert.NotNull(database.Element("StoredProcedures"));
    }

    [Fact]
    public async Task WriteTablesAsync_WithNullList_DoesNotThrow()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);

        // Act & Assert
        await writer.WriteTablesAsync(null);
        await writer.SaveAsync();
    }

    [Fact]
    public async Task WriteColumnsAsync_WithNullList_DoesNotThrow()
    {
        // Arrange
        using var writer = new XmlSchemaWriter(_outputPath);

        // Act & Assert
        await writer.WriteColumnsAsync(null);
        await writer.SaveAsync();
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}

