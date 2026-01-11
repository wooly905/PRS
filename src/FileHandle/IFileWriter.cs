namespace PRS.FileHandle;

public interface IFileWriter
{
    public Task WriteLineAsync(string input);
    public void Dispose();
}
