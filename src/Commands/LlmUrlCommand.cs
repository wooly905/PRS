using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class LlmUrlCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        if (args == null || args.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs llmurl \"https://your-endpoint/openai/v1\"");
            return;
        }

        string endpoint = args[1];

        await CommandHelper.UpsertConfigKeyValueAsync(Global.LlmConfigFilePath, Global.LlmUrlConfigKey, endpoint);
        _display.ShowInfo("LLM endpoint has been set.");
    }
}


