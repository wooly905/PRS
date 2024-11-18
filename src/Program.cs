using PRS.Commands;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;
using Spectre.Console;

namespace PRS;

static class Program
{
    static async Task Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            DisplayHelp();
            return;
        }

        string command = args[0];
        IDisplay display = new LcdMonitor();
        IDatabase database = new PrsDatabase();
        IFileProvider file = new FileProvider();

        if (CommandProvider.TryGetProvider(command,
                                           display,
                                           database,
                                           file,
                                           out ICommand value))
        {
            await value.RunAsync(args);
        }
        else
        {
            display.ShowError("Unknown command");
        }
    }

    static void DisplayHelp()
    {
        Console.WriteLine();

        Console.WriteLine("Usage: prs [options] [argument]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("scs     Show MS SQL Server connection string.");
        Console.WriteLine("wcs     Write MS SQL Server connection string.");
        Console.WriteLine("dds     Dump db schema to local machine.");
        Console.WriteLine("ft      Find table(s) (view).");
        Console.WriteLine("fc      Find column(s).");
        Console.WriteLine("ftc     Find column(s) in some table (view).");
        Console.WriteLine("fsp     Find stored procedure.");
        Console.WriteLine("sc      Show all columns in a table.");

        Console.WriteLine();
    }
}
