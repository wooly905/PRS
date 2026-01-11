namespace PRS.FileHandle;

internal class SchemaFileWriter : IDisposable, IFileWriter
{
    private readonly StreamWriter _writer;

    public SchemaFileWriter(string filePath)
    {
        _writer = new StreamWriter(filePath)
        {
            AutoFlush = true
        };
    }

    public void Dispose()
    {
        if (_writer != null)
        {
            _writer.Close();
            _writer.Dispose();
        }
    }

    public async Task WriteLineAsync(string input)
    {
        await _writer.WriteLineAsync(input);
    }
}
