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

    // New high-level interfaces for schema reading/writing (XML format)
    public ISchemaReader GetSchemaReader(string file)
    {
        return new XmlSchemaReader(file);
    }

    public ISchemaWriter GetSchemaWriter(string file)
    {
        return new XmlSchemaWriter(file);
    }
}
