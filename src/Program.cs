using System.Reflection;
using PRS.Commands;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS;

static class Program
{
    static async Task Main(string[] args)
    {
        if (args == null
            || args.Length == 0
            || string.Equals(args[0], "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(args[0], "-h", StringComparison.OrdinalIgnoreCase)
            || string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase))
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
        string fullVersion = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";
        // Strip the +commitHash suffix appended by .NET SDK
        string version = fullVersion.IndexOf('+') is int i and >= 0 ? fullVersion[..i] : fullVersion;

        Console.WriteLine();
        Console.WriteLine($"prs - SQL Server Schema Query Tool v{version}");
        Console.WriteLine();
        Console.WriteLine("Usage: prs <command> [arguments] [-f <format>]");
        Console.WriteLine();
        Console.WriteLine("Setup:");
        Console.WriteLine("  scs                           Show saved connection string");
        Console.WriteLine("  wcs <connection-string>       Save connection string");
        Console.WriteLine("  dds <schema-name>             Dump database schema to local file");
        Console.WriteLine();
        Console.WriteLine("Schema Management:");
        Console.WriteLine("  ls                            List all saved schemas (* = active)");
        Console.WriteLine("  use <schema-name>             Switch active schema");
        Console.WriteLine("  rm  <schema-name>             Remove a saved schema");
        Console.WriteLine();
        Console.WriteLine("Query (partial match, case-insensitive):");
        Console.WriteLine("  ft  <keyword>                 Find tables by name");
        Console.WriteLine("  fc  <keyword>                 Find columns by name across all tables");
        Console.WriteLine("  ftc <table> <column>          Find columns in matching tables");
        Console.WriteLine("  sc  <table-name>              Show all columns in a table (exact match)");
        Console.WriteLine("  fsp <keyword>                 Find stored procedures by name");
        Console.WriteLine();
        Console.WriteLine("Output Format [-f]:");
        Console.WriteLine("  table   Formatted table with borders (default for all commands)");
        Console.WriteLine("  json    JSON structured format       (all query commands)");
        Console.WriteLine("  text    Plain text format             (all query commands)");
        Console.WriteLine("  ddl     SQL DDL (CREATE TABLE)        (sc only)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  prs dds mydb                  Dump schema from connected database");
        Console.WriteLine("  prs ft user                   Find tables containing \"user\"");
        Console.WriteLine("  prs sc Orders -f ddl          Show Orders table as CREATE TABLE DDL");
        Console.WriteLine("  prs fc email -f json          Find columns matching \"email\" in JSON");
        Console.WriteLine("  prs ftc Order Status -f text  Find \"Status\" columns in \"Order\" tables");
        Console.WriteLine();
    }
}
