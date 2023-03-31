using System;
using System.IO;
using System.Threading.Tasks;

namespace PRS.FileHandle;

internal class SchemaFileReader : IDisposable, IFileReader
{
    private readonly StreamReader _reader;

    public SchemaFileReader(string schemaFilePath)
    {
        _reader = new StreamReader(schemaFilePath);
    }

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
        return await _reader.ReadLineAsync().ConfigureAwait(false);
    }
}
