using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class ShowConnectionStringCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // verify schema file exists. if not, show no schema file error and ask to run dump command.
        string output = await CommandHelper.GetConnectionStringAsync(_display, _fileProvider);

        if (output == null)
        {
            _display.ShowError("Connection string doesn't exist. Please set it first.");
            return;
        }

        _display.ShowInfo($"Connection string = {output}");
    }
}
