namespace PRS.FileHandle;

internal class FileProvider : IFileProvider
{
    public IFileReader GetFileReader(string file)
    {
        return new SchemaFileReader(file);
    }

    public IFileWriter GetFileWriter(string file)
    {
        return new SchemaFileWriter(file);
    }
}
