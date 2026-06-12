namespace PRS.FileHandle;

internal class FileProvider : IFileProvider
{
    // Legacy interfaces for line-by-line reading/writing
    public IFileReader GetFileReader(string file)
    {
        return new SchemaFileReader(file);
    }

    public IFileWriter GetFileWriter(string file)
    {
        return new SchemaFileWriter(file);
    }

    // New high-level interfaces for schema reading/writing (JSON or Markdown format)
    public ISchemaReader GetSchemaReader(string file)
    {
        if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonSchemaReader(file);
        }
        return new MarkdownSchemaReader(file);
    }

    public ISchemaWriter GetSchemaWriter(string file)
    {
        if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonSchemaWriter(file);
        }
        return new MarkdownSchemaWriter(file);
    }
}
