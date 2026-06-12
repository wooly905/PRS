using Xunit;
using PRS.Tests.TestHelpers;

namespace PRS.Tests;

public class GlobalTests : IDisposable
{
    private readonly string _originalAppData;

    public GlobalTests()
    {
        _originalAppData = Environment.GetEnvironmentVariable("APPDATA")!;
        TestFileHelper.ResetTempPath();
        Environment.SetEnvironmentVariable("APPDATA", TestFileHelper.GetTempPath());
    }

    // ── SafeFileName ────────────────────────────────────────────────

    [Fact]
    public void SafeFileName_EmptyInput_ReturnsUnderscore()
    {
        Assert.Equal("_", Global.SafeFileName(""));
    }

    [Fact]
    public void SafeFileName_WhitespaceInput_ReturnsUnderscore()
    {
        Assert.Equal("_", Global.SafeFileName("   "));
    }

    [Fact]
    public void SafeFileName_NormalInput_ReturnsSame()
    {
        Assert.Equal("myserver", Global.SafeFileName("myserver"));
    }

    [Fact]
    public void SafeFileName_DotsReplacedWithUnderscore()
    {
        Assert.Equal("my_server_com", Global.SafeFileName("my.server.com"));
    }

    [Fact]
    public void SafeFileName_InvalidCharsReplacedWithUnderscore()
    {
        // Backslash and colon are invalid filename chars
        string result = Global.SafeFileName("server\\instance:1433");
        Assert.DoesNotContain("\\", result);
        Assert.DoesNotContain(":", result);
        Assert.Contains("_", result);
    }

    // ── CreateSchemaFileNameFromConnectionString ────────────────────

    [Fact]
    public void CreateSchemaFileName_EmptyString_ReturnsDefault()
    {
        string result = Global.CreateSchemaFileNameFromConnectionString("");
        Assert.Equal("schema.json", result);
    }

    [Fact]
    public void CreateSchemaFileName_NullString_ReturnsDefault()
    {
        string result = Global.CreateSchemaFileNameFromConnectionString(null!);
        Assert.Equal("schema.json", result);
    }

    [Fact]
    public void CreateSchemaFileName_ValidConnectionString_ReturnsJsonExtension()
    {
        string result = Global.CreateSchemaFileNameFromConnectionString(
            "Server=myserver;Database=mydb;Integrated Security=true;");
        Assert.EndsWith(".schema.json", result);
        Assert.Contains("myserver", result);
        Assert.Contains("mydb", result);
    }

    [Fact]
    public void CreateSchemaFileName_DataSourceKey_ExtractsServer()
    {
        string result = Global.CreateSchemaFileNameFromConnectionString(
            "Data Source=srv1;Initial Catalog=db1;");
        Assert.EndsWith(".schema.json", result);
        Assert.Contains("srv1", result);
        Assert.Contains("db1", result);
    }

    [Fact]
    public void CreateSchemaFileName_MissingServerAndDb_UsesFallback()
    {
        string result = Global.CreateSchemaFileNameFromConnectionString(
            "SomeKey=SomeValue;");
        Assert.EndsWith(".schema.json", result);
        Assert.Contains("server", result);
        Assert.Contains("database", result);
    }

    // ── ResolveActiveSchemaFilePath (via SchemaFilePath property) ───

    [Fact]
    public void SchemaFilePath_ActivePointerPointsToExistingJson_ReturnsJsonPath()
    {
        // Create the .json schema file
        string schemasDir = Global.SchemasDirectory;
        Directory.CreateDirectory(schemasDir);
        string jsonFile = Path.Combine(schemasDir, "test.schema.json");
        File.WriteAllText(jsonFile, "{}");

        // Set active pointer
        Global.SetActiveSchema("test.schema.json");

        Assert.Equal(jsonFile, Global.SchemaFilePath);
    }

    [Fact]
    public void SchemaFilePath_ActivePointerJsonNotExist_FallsBackToMd()
    {
        // Only create the .md version
        string schemasDir = Global.SchemasDirectory;
        Directory.CreateDirectory(schemasDir);
        string mdFile = Path.Combine(schemasDir, "test.schema.md");
        File.WriteAllText(mdFile, "# Schema");

        // Pointer points to .json which doesn't exist
        Global.SetActiveSchema("test.schema.json");

        Assert.Equal(mdFile, Global.SchemaFilePath);
    }

    [Fact]
    public void SchemaFilePath_ActivePointerJsonNotExist_MdNotExist_ContinuesFallback()
    {
        // Pointer points to .json, neither .json nor .md exist
        Directory.CreateDirectory(Global.SchemasDirectory);
        Global.SetActiveSchema("nonexistent.schema.json");

        // Should fall through to legacy/default paths
        string result = Global.SchemaFilePath;
        Assert.NotNull(result);
        Assert.False(result.Contains("nonexistent"));
    }

    [Fact]
    public void SchemaFilePath_NoPointer_LegacyJsonExists_ReturnsLegacyJson()
    {
        // Create legacy json in SchemaFileDirectory
        Directory.CreateDirectory(Global.SchemaFileDirectory);
        string legacyJson = Path.Combine(Global.SchemaFileDirectory, "schema.json");
        File.WriteAllText(legacyJson, "{}");

        // No active pointer (or empty)
        string pointerPath = Global.ActiveSchemaPointerFilePath;
        if (File.Exists(pointerPath)) File.Delete(pointerPath);

        Assert.Equal(legacyJson, Global.SchemaFilePath);
    }

    [Fact]
    public void SchemaFilePath_NoPointer_LegacyMdExists_ReturnsLegacyMd()
    {
        Directory.CreateDirectory(Global.SchemaFileDirectory);
        string legacyMd = Path.Combine(Global.SchemaFileDirectory, "schema.md");
        File.WriteAllText(legacyMd, "# Schema");

        // No active pointer
        string pointerPath = Global.ActiveSchemaPointerFilePath;
        if (File.Exists(pointerPath)) File.Delete(pointerPath);

        Assert.Equal(legacyMd, Global.SchemaFilePath);
    }

    [Fact]
    public void SchemaFilePath_NoPointer_SchemasJsonExists_ReturnsSchemasJson()
    {
        // No legacy files, create schema.json in schemas dir
        string schemasDir = Global.SchemasDirectory;
        Directory.CreateDirectory(schemasDir);
        string schemasJson = Path.Combine(schemasDir, "schema.json");
        File.WriteAllText(schemasJson, "{}");

        // No active pointer
        string pointerPath = Global.ActiveSchemaPointerFilePath;
        if (File.Exists(pointerPath)) File.Delete(pointerPath);

        Assert.Equal(schemasJson, Global.SchemaFilePath);
    }

    [Fact]
    public void SchemaFilePath_NoPointer_SchemasMdExists_ReturnsSchemasDir()
    {
        string schemasDir = Global.SchemasDirectory;
        Directory.CreateDirectory(schemasDir);
        string schemasMd = Path.Combine(schemasDir, "schema.md");
        File.WriteAllText(schemasMd, "# Schema");

        string pointerPath = Global.ActiveSchemaPointerFilePath;
        if (File.Exists(pointerPath)) File.Delete(pointerPath);

        Assert.Equal(schemasMd, Global.SchemaFilePath);
    }

    [Fact]
    public void SchemaFilePath_NoFilesExist_ReturnsDefaultInSchemasDir()
    {
        Directory.CreateDirectory(Global.SchemasDirectory);

        string pointerPath = Global.ActiveSchemaPointerFilePath;
        if (File.Exists(pointerPath)) File.Delete(pointerPath);

        string result = Global.SchemaFilePath;
        Assert.EndsWith("schema.json", result);
        Assert.Contains("schemas", result);
    }

    // ── GetActiveSchemaName / SetActiveSchema ─────────────────────

    [Fact]
    public void SetActiveSchema_ThenGetActiveSchemaName_RoundTrips()
    {
        Global.SetActiveSchema("mydb.schema.json");
        Assert.Equal("mydb.schema.json", Global.GetActiveSchemaName());
    }

    [Fact]
    public void SetActiveSchema_Null_ClearsPointer()
    {
        Global.SetActiveSchema("mydb.schema.json");
        Global.SetActiveSchema(null);
        Assert.Null(Global.GetActiveSchemaName());
    }

    [Fact]
    public void GetActiveSchemaName_NoPointerFile_ReturnsNull()
    {
        string pointerPath = Global.ActiveSchemaPointerFilePath;
        if (File.Exists(pointerPath)) File.Delete(pointerPath);

        Assert.Null(Global.GetActiveSchemaName());
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("APPDATA", _originalAppData);
        TestFileHelper.CleanupTempFiles();
    }
}
