using System.Threading.Tasks;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands
{
    internal class ShowConnectionStringCommand : ICommand
    {
        private readonly IDisplay _display;
        private readonly IFileProvider _fileProvider;

        public ShowConnectionStringCommand(IDisplay display, IFileProvider fileProvider)
        {
            _display = display;
            _fileProvider = fileProvider;
        }

        public async Task RunAsync(string[] args)
        {
            // verify schema file exists. if not, show no schema file error and ask to run dump command.
            string output = await CommandHelper.GetConnectionStringAsync(_display, _fileProvider).ConfigureAwait(false);

            if (output == null)
            {
                _display.ShowError("Connection string doesn't exist. Please set it first.");
                return;
            }

            _display.ShowInfo($"Connection string = {output}");
        }
    }
}
