namespace PRS.FileHandle;

internal interface IFileProvider
{
    // Legacy interfaces for line-by-line reading/writing
    public IFileReader GetFileReader(string file);
    public IFileWriter GetFileWriter(string file);

    // New high-level interfaces for schema reading/writing (Markdown format)
    public ISchemaReader GetSchemaReader(string file);
    public ISchemaWriter GetSchemaWriter(string file);
}
