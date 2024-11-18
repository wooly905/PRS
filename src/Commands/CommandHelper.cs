using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

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
        string output = await reader.ReadLineAsync();
        reader.Dispose();

        return output;
    }

    public static void PrintColumns(IEnumerable<ColumnModel> models, IDisplay display)
    {
        display.DisplayColumns(models);
    }

    public static void PrintTables(IEnumerable<TableModel> models, IDisplay display)
    {
        display.DisplayTables(models);
    }

    public static void PrintModel(IEnumerable<string> models, IDisplay display)
    {
        display.ShowInfo("Name ===");

        foreach (string m in models)
        {
            display.ShowInfo(m);
        }
    }
}
