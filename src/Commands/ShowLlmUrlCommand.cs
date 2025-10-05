using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class ShowLlmUrlCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        string value = await CommandHelper.GetConfigValueAsync(Global.LlmConfigFilePath, Global.LlmUrlConfigKey);
        if (string.IsNullOrWhiteSpace(value))
        {
            _display.ShowError("LLM endpoint is not set.");
            return;
        }
        _display.ShowInfo($"LLM endpoint = {value}");
    }
}


