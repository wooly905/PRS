namespace PRS.FileHandle;

internal class SchemaFileReader(string schemaFilePath) : IDisposable, IFileReader
{
    private readonly StreamReader _reader = new(schemaFilePath);

    public void Dispose()
    {
        if (_reader != null)
        {
            _reader.Close();
            _reader.Dispose();
        }
    }

    public async Task<string> ReadLineAsync()
    {
        return await _reader.ReadLineAsync();
    }
}
