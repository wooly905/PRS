using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal static class CommandProvider
{
    public static bool TryGetProvider(string input,
                                      IDisplay display,
                                      IDatabase database,
                                      IFileProvider fileProvider,
                                      out ICommand command)
    {
        if (string.Equals(input, "scs", StringComparison.OrdinalIgnoreCase))
        {
            command = new ShowConnectionStringCommand(display, fileProvider);
            return true;
        }

        if (string.Equals(input, "wcs", StringComparison.OrdinalIgnoreCase))
        {
            command = new WriteConnectionStringCommand(display, fileProvider);
            return true;
        }

        if (string.Equals(input, "dds", StringComparison.OrdinalIgnoreCase))
        {
            command = new DumpDatabaseSchemaCommand(display, database, fileProvider);
            return true;
        }

        if (string.Equals(input, "ls", StringComparison.OrdinalIgnoreCase))
        {
            command = new ListSchemasCommand(display);
            return true;
        }

        if (string.Equals(input, "use", StringComparison.OrdinalIgnoreCase))
        {
            command = new UseSchemaCommand(display);
            return true;
        }

        if (string.Equals(input, "rm", StringComparison.OrdinalIgnoreCase))
        {
            command = new RemoveSchemaCommand(display);
            return true;
        }

        if (string.Equals(input, "ft", StringComparison.OrdinalIgnoreCase))
        {
            command = new FindTableCommand(display, fileProvider);
            return true;
        }

        if (string.Equals(input, "fc", StringComparison.OrdinalIgnoreCase))
        {
            command = new FindColumnCommand(display, fileProvider);
            return true;
        }

        if (string.Equals(input, "ftc", StringComparison.OrdinalIgnoreCase))
        {
            command = new FindTableColumnCommand(display, fileProvider);
            return true;
        }

        if (string.Equals(input, "fsp", StringComparison.OrdinalIgnoreCase))
        {
            command = new FindStoredProcedureCommand(display, fileProvider);
            return true;
        }

        if (string.Equals(input, "sc", StringComparison.OrdinalIgnoreCase))
        {
            command = new ShowAllColumnsCommand(display, fileProvider);
            return true;
        }

        command = null;
        return false;
    }
}
