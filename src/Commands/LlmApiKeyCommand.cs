using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class LlmApiKeyCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        if (args == null || args.Length != 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs llmapikey your_api_key");
            return;
        }

        string apiKey = args[1];

        await CommandHelper.UpsertConfigKeyValueAsync(Global.LlmConfigFilePath, Global.LlmApiKeyConfigKey, apiKey);
        _display.ShowInfo("LLM API key has been set.");
    }
}


