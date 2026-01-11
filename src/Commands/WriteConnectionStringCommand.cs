using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class WriteConnectionStringCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // verify args
        if (args == null || args.Length != 2)
        {
            _display.ShowError("Connection string argument is missing.");
            return;
        }

        string cs = args[1];

        // verify schema file exists. if no, create schema file and write connection string.
        if (Directory.Exists(Global.SchemaFileDirectory))
        {
            if (File.Exists(Global.ConnectionStringFilePath))
            {
                File.Delete(Global.ConnectionStringFilePath);
            }
        }
        else
        {
            Directory.CreateDirectory(Global.SchemaFileDirectory);
        }

        IFileWriter writer = _fileProvider.GetFileWriter(Global.ConnectionStringFilePath);
        await writer.WriteLineAsync(cs);
        writer.Dispose();

        _display.ShowInfo("Connection string has been set.");
    }
}
