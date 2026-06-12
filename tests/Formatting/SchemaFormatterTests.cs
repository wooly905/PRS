using PRS.Database;
using PRS.Formatting;
using Xunit;

namespace PRS.Tests.Formatting;

public class SchemaFormatterTests
{
    // ── ParseFormat ─────────────────────────────────────────────────

    [Theory]
    [InlineData("table", OutputFormat.Table)]
    [InlineData("TABLE", OutputFormat.Table)]
    [InlineData("ddl", OutputFormat.Ddl)]
    [InlineData("DDL", OutputFormat.Ddl)]
    [InlineData("json", OutputFormat.Json)]
    [InlineData("JSON", OutputFormat.Json)]
    [InlineData("text", OutputFormat.Text)]
    [InlineData("TEXT", OutputFormat.Text)]
    public void ParseFormat_ValidValues_ReturnsExpected(string input, OutputFormat expected)
    {
        Assert.Equal(expected, SchemaFormatter.ParseFormat(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("csv")]
    [InlineData("invalid")]
    public void ParseFormat_InvalidValues_ReturnsNull(string? input)
    {
        Assert.Null(SchemaFormatter.ParseFormat(input));
    }

    // ── FormatTables ────────────────────────────────────────────────

    private static List<TableModel> SampleTables() =>
    [
        new() { TableSchema = "dbo", TableName = "Users", TableType = "BASE TABLE" },
        new() { TableSchema = "dbo", TableName = "Orders", TableType = "BASE TABLE" }
    ];

    [Fact]
    public void FormatTables_Json_ReturnsValidJson()
    {
        string result = SchemaFormatter.FormatTables(SampleTables(), OutputFormat.Json);
        Assert.Contains("\"name\": \"Users\"", result);
        Assert.Contains("\"name\": \"Orders\"", result);
        Assert.Contains("\"schema\": \"dbo\"", result);
    }

    [Fact]
    public void FormatTables_Text_ContainsTableInfo()
    {
        string result = SchemaFormatter.FormatTables(SampleTables(), OutputFormat.Text);
        Assert.Contains("Found 2 table(s)", result);
        Assert.Contains("Users", result);
        Assert.Contains("Orders", result);
    }

    [Fact]
    public void FormatTables_Text_EmptyList_ReturnsNotFound()
    {
        string result = SchemaFormatter.FormatTables([], OutputFormat.Text);
        Assert.Contains("No tables found", result);
    }

    // ── FormatColumns ───────────────────────────────────────────────

    private static List<ColumnModel> SampleColumns() =>
    [
        new()
        {
            TableSchema = "dbo", TableName = "Users", ColumnName = "UserId",
            DataType = "int", IsNullable = "NO", OrdinalPosition = "1",
            ColumnDefault = "", CharacterMaximumLength = "",
            ForeignKeyName = "", ReferencedTableName = "", ReferencedColumnName = ""
        },
        new()
        {
            TableSchema = "dbo", TableName = "Users", ColumnName = "Email",
            DataType = "nvarchar", IsNullable = "YES", OrdinalPosition = "2",
            ColumnDefault = "", CharacterMaximumLength = "255",
            ForeignKeyName = "", ReferencedTableName = "", ReferencedColumnName = ""
        }
    ];

    [Fact]
    public void FormatColumns_Json_ContainsColumnData()
    {
        string result = SchemaFormatter.FormatColumns(SampleColumns(), OutputFormat.Json);
        Assert.Contains("\"column\": \"UserId\"", result);
        Assert.Contains("\"column\": \"Email\"", result);
        Assert.Contains("\"dataType\": \"int\"", result);
    }

    [Fact]
    public void FormatColumns_Text_ContainsColumnData()
    {
        string result = SchemaFormatter.FormatColumns(SampleColumns(), OutputFormat.Text);
        Assert.Contains("Found 2 column(s)", result);
        Assert.Contains("UserId", result);
        Assert.Contains("Email", result);
    }

    [Fact]
    public void FormatColumns_Text_EmptyList_ReturnsNotFound()
    {
        string result = SchemaFormatter.FormatColumns(new List<ColumnModel>(), OutputFormat.Text);
        Assert.Contains("No columns found", result);
    }

    [Fact]
    public void FormatColumns_Json_WithForeignKey_IncludesFkData()
    {
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo", TableName = "Orders", ColumnName = "UserId",
                DataType = "int", IsNullable = "NO", OrdinalPosition = "1",
                ColumnDefault = "", CharacterMaximumLength = "",
                ForeignKeyName = "FK_Orders_Users",
                ReferencedTableSchema = "dbo",
                ReferencedTableName = "Users",
                ReferencedColumnName = "UserId"
            }
        };

        string result = SchemaFormatter.FormatColumns(columns, OutputFormat.Json);
        Assert.Contains("\"referencedTable\": \"Users\"", result);
        Assert.Contains("\"referencedColumn\": \"UserId\"", result);
    }

    // ── FormatTableSchema ───────────────────────────────────────────

    private static List<ColumnModel> SampleTableSchemaColumns() =>
    [
        new()
        {
            TableSchema = "dbo", TableName = "Users", ColumnName = "UserId",
            DataType = "int", IsNullable = "NO", OrdinalPosition = "1",
            IsPrimaryKey = true, IsUnique = true, IsIdentity = true,
            IdentitySeed = "1", IdentityIncrement = "1",
            ColumnDefault = "", CharacterMaximumLength = "",
            ForeignKeyName = "", ReferencedTableName = "", ReferencedColumnName = ""
        },
        new()
        {
            TableSchema = "dbo", TableName = "Users", ColumnName = "UserName",
            DataType = "nvarchar", IsNullable = "NO", OrdinalPosition = "2",
            ColumnDefault = "", CharacterMaximumLength = "100",
            ForeignKeyName = "", ReferencedTableName = "", ReferencedColumnName = ""
        }
    ];

    [Fact]
    public void FormatTableSchema_Ddl_ContainsCreateTable()
    {
        string result = SchemaFormatter.FormatTableSchema(
            SampleTableSchemaColumns(), "Users", "dbo", true, OutputFormat.Ddl);

        Assert.Contains("CREATE TABLE dbo.Users", result);
        Assert.Contains("UserId int", result);
        Assert.Contains("NOT NULL", result);
        Assert.Contains("IDENTITY(1,1)", result);
        Assert.Contains("PRIMARY KEY", result);
    }

    [Fact]
    public void FormatTableSchema_Ddl_NotFound_ReturnsComment()
    {
        string result = SchemaFormatter.FormatTableSchema(
            new List<ColumnModel>(), "Missing", "dbo", false, OutputFormat.Ddl);

        Assert.StartsWith("--", result);
        Assert.Contains("Missing", result);
    }

    [Fact]
    public void FormatTableSchema_Json_ContainsTableAndColumns()
    {
        string result = SchemaFormatter.FormatTableSchema(
            SampleTableSchemaColumns(), "Users", "dbo", true, OutputFormat.Json);

        Assert.Contains("\"tableName\": \"Users\"", result);
        Assert.Contains("\"found\": true", result);
        Assert.Contains("\"name\": \"UserId\"", result);
    }

    [Fact]
    public void FormatTableSchema_Json_NotFound_ReturnsFoundFalse()
    {
        string result = SchemaFormatter.FormatTableSchema(
            new List<ColumnModel>(), "Missing", "dbo", false, OutputFormat.Json);

        Assert.Contains("\"found\": false", result);
    }

    [Fact]
    public void FormatTableSchema_Text_ContainsTableInfo()
    {
        string result = SchemaFormatter.FormatTableSchema(
            SampleTableSchemaColumns(), "Users", "dbo", true, OutputFormat.Text);

        Assert.Contains("Table:", result);
        Assert.Contains("Users", result);
        Assert.Contains("Columns (2)", result);
    }

    [Fact]
    public void FormatTableSchema_Text_NotFound_ReturnsMessage()
    {
        string result = SchemaFormatter.FormatTableSchema(
            new List<ColumnModel>(), "Missing", "dbo", false, OutputFormat.Text);

        Assert.Contains("not found", result);
    }

    // ── FormatStoredProcedures ──────────────────────────────────────

    [Fact]
    public void FormatStoredProcedures_Json_ReturnsJsonArray()
    {
        var procs = new List<string> { "sp_GetUsers", "sp_CreateOrder" };
        string result = SchemaFormatter.FormatStoredProcedures(procs, OutputFormat.Json);

        Assert.Contains("sp_GetUsers", result);
        Assert.Contains("sp_CreateOrder", result);
        Assert.StartsWith("[", result.Trim());
    }

    [Fact]
    public void FormatStoredProcedures_Text_ContainsProcInfo()
    {
        var procs = new List<string> { "sp_GetUsers", "sp_CreateOrder" };
        string result = SchemaFormatter.FormatStoredProcedures(procs, OutputFormat.Text);

        Assert.Contains("Found 2 stored procedure(s)", result);
        Assert.Contains("sp_GetUsers", result);
    }

    [Fact]
    public void FormatStoredProcedures_Text_EmptyList_ReturnsNotFound()
    {
        string result = SchemaFormatter.FormatStoredProcedures(new List<string>(), OutputFormat.Text);
        Assert.Contains("No stored procedures found", result);
    }

    // ── FormatTableSchema DDL edge cases ────────────────────────────

    [Fact]
    public void FormatTableSchema_Ddl_WithForeignKey_ContainsFkConstraint()
    {
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo", TableName = "Orders", ColumnName = "UserId",
                DataType = "int", IsNullable = "NO", OrdinalPosition = "1",
                ForeignKeyName = "FK_Orders_Users",
                ReferencedTableSchema = "dbo",
                ReferencedTableName = "Users",
                ReferencedColumnName = "UserId",
                ColumnDefault = "", CharacterMaximumLength = ""
            }
        };

        string result = SchemaFormatter.FormatTableSchema(
            columns, "Orders", "dbo", true, OutputFormat.Ddl);

        Assert.Contains("FOREIGN KEY", result);
        Assert.Contains("REFERENCES dbo.Users(UserId)", result);
    }

    [Fact]
    public void FormatTableSchema_Ddl_WithUniqueNonPk_ContainsUniqueConstraint()
    {
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo", TableName = "Users", ColumnName = "Email",
                DataType = "nvarchar", IsNullable = "NO", OrdinalPosition = "1",
                IsUnique = true, IsPrimaryKey = false,
                ColumnDefault = "", CharacterMaximumLength = "255",
                ForeignKeyName = "", ReferencedTableName = "", ReferencedColumnName = ""
            }
        };

        string result = SchemaFormatter.FormatTableSchema(
            columns, "Users", "dbo", true, OutputFormat.Ddl);

        Assert.Contains("UNIQUE", result);
    }

    [Fact]
    public void FormatTableSchema_Ddl_WithMaxLength_FormatsCorrectly()
    {
        var columns = new List<ColumnModel>
        {
            new()
            {
                TableSchema = "dbo", TableName = "T", ColumnName = "Col",
                DataType = "nvarchar", IsNullable = "YES", OrdinalPosition = "1",
                CharacterMaximumLength = "-1",
                ColumnDefault = "",
                ForeignKeyName = "", ReferencedTableName = "", ReferencedColumnName = ""
            }
        };

        string result = SchemaFormatter.FormatTableSchema(
            columns, "T", "dbo", true, OutputFormat.Ddl);

        Assert.Contains("nvarchar(MAX)", result);
    }
}
