using PRS.FileHandle;
using Xunit;

namespace PRS.Tests.FileHandle;

public class FileProviderTests : IDisposable
{
    private readonly string _tempDir;

    public FileProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PRS.Tests.FP", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void GetSchemaReader_JsonFile_ReturnsJsonSchemaReader()
    {
        // Create a minimal valid JSON schema file
        var jsonPath = Path.Combine(_tempDir, "test.schema.json");
        File.WriteAllText(jsonPath, "{\"connectionString\":\"\",\"tables\":[],\"storedProcedures\":[]}");

        var provider = new FileProvider();
        using var reader = provider.GetSchemaReader(jsonPath);

        Assert.IsType<JsonSchemaReader>(reader);
    }

    [Fact]
    public void GetSchemaReader_MdFile_ReturnsMarkdownSchemaReader()
    {
        var mdPath = Path.Combine(_tempDir, "test.schema.md");
        File.WriteAllText(mdPath, "# Schema\n## Tables\n");

        var provider = new FileProvider();
        using var reader = provider.GetSchemaReader(mdPath);

        Assert.IsType<MarkdownSchemaReader>(reader);
    }

    [Fact]
    public void GetSchemaWriter_JsonFile_ReturnsJsonSchemaWriter()
    {
        var jsonPath = Path.Combine(_tempDir, "output.schema.json");

        var provider = new FileProvider();
        var writer = provider.GetSchemaWriter(jsonPath);

        Assert.IsType<JsonSchemaWriter>(writer);
        writer.Dispose();
    }

    [Fact]
    public void GetSchemaWriter_MdFile_ReturnsMarkdownSchemaWriter()
    {
        var mdPath = Path.Combine(_tempDir, "output.schema.md");

        var provider = new FileProvider();
        var writer = provider.GetSchemaWriter(mdPath);

        Assert.IsType<MarkdownSchemaWriter>(writer);
        writer.Dispose();
    }

    [Fact]
    public void GetSchemaReader_CaseInsensitiveJsonExtension_ReturnsJsonReader()
    {
        var jsonPath = Path.Combine(_tempDir, "test.schema.JSON");
        File.WriteAllText(jsonPath, "{\"connectionString\":\"\",\"tables\":[],\"storedProcedures\":[]}");

        var provider = new FileProvider();
        using var reader = provider.GetSchemaReader(jsonPath);

        Assert.IsType<JsonSchemaReader>(reader);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }
}
