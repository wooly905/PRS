using System.IO;
using System.Threading.Tasks;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands
{
    internal static class CommandHelper
    {
        public static async Task<string> GetConnectionStringAsync(IDisplay display, IFileProvider fileProvider)
        {
            if (!File.Exists(Global.ConnectionStringFilePath))
            {
                display.ShowError("Connection string doesn't exist. Please set it first.");
                return string.Empty;
            }

            IFileReader reader = fileProvider.GetFileReader(Global.ConnectionStringFilePath);
            string output = await reader.ReadLineAsync().ConfigureAwait(false);
            reader.Dispose();

            return output;
        }
    }
}
