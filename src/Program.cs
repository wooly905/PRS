using PRS.Commands;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

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
        Console.WriteLine("ls      List saved schemas and show active one.");
        Console.WriteLine("use     Switch active schema. prs use [schema name]");
        Console.WriteLine("rm      Remove a schema. prs rm [schema name]");
        Console.WriteLine("ft      Find table(s) (view).");
        Console.WriteLine("fc      Find column(s).");
        Console.WriteLine("ftc     Find column(s) in some table (view).");
        Console.WriteLine("fsp     Find stored procedure.");
        Console.WriteLine("sc      Show all columns in a table.");
        Console.WriteLine("erd     Show ERD around a table. prs erd [table name]");
        Console.WriteLine("ai      Generate T-SQL from natural language: prs ai \"question...\"");
        Console.WriteLine("lu      Set LLM endpoint URL. prs lu \"https://.../openai/v1\"");
        Console.WriteLine("lk      Set LLM API key. prs lk your_api_key");
        Console.WriteLine("slu     Show current LLM endpoint URL.");
        Console.WriteLine("slk     Show current LLM API key.");

        Console.WriteLine();
    }
}
